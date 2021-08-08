#include "ImageProcessor.h"
#include <string>
#include <cmath>
#include <climits>
#include <cstdlib>
#include <ctime>
#include "ImageUtils.h"
#include <fstream>

ImageProcessor::ImageProcessor()
{
	_multiplier = 0;
	_power = 0;
	_gainFactor = 0;
	_smallDetailGainFactor = 0;
	_performanceRate = 0;
}

ImageProcessor::~ImageProcessor()
{
}

ComplexityCalculationResult* ImageProcessor::CalculateObjectComplexity(const ComplexityCalculationData& data)
{
	auto result = new ComplexityCalculationResult();
	result->Status = ComplexityCalculationStatus::Undefined;
	result->LaborIntensity = -1;
	result->TotalComplexity = -1;
	result->CalculatedComplexity = -1;
	result->PresumedComplexity = -1;

	if (data.ColorImage == nullptr || data.ColorImage->Data == nullptr)
	{
		result->Status = ComplexityCalculationStatus::ImageError;
		return result;
	}

	const auto image = GetGrayscaleImage(*(data.ColorImage));

	cv::Mat paddedImage;
	copyMakeBorder(image, paddedImage, 50, 50, 50, 50, cv::BORDER_REPLICATE);

	const Contour& objectContour = GetTargetContourFromImage(paddedImage);
	if (objectContour.size() == 0)
	{
		result->Status = ComplexityCalculationStatus::NoObjectFound;
		return result;
	}

	const auto minBoundingRect = cv::minAreaRect(objectContour);

	// Ht
	const float calculatedComplexity = GetCalculatedComplexity(paddedImage, objectContour, minBoundingRect, std::string(data.DebugFileName));
	if (calculatedComplexity < 0)
	{
		result->Status = ComplexityCalculationStatus::CalculationError;
		return result;
	}

	// Hp
	const float presumedComplexity = GetPresumedComplexity(data.PartWidth, data.PartHeight);
	if (presumedComplexity < 0)
	{
		result->Status = ComplexityCalculationStatus::CalculationError;
		return result;
	}

	// H
	const float totalComplexity = GetTotalComplexity(calculatedComplexity, presumedComplexity);
	if (totalComplexity < 0)
	{
		result->Status = ComplexityCalculationStatus::CalculationError;
		return result;
	}

	// T
	const int partArea = data.PartWidth * data.PartHeight;
	const float laborIntensity = GetLaborIntensity(partArea, totalComplexity);
	if (laborIntensity < 0)
	{
		result->Status = ComplexityCalculationStatus::CalculationError;
		return result;
	}

	result->Status = ComplexityCalculationStatus::Success;
	result->LaborIntensity = laborIntensity;
	result->TotalComplexity = totalComplexity;
	result->CalculatedComplexity = calculatedComplexity;
	result->PresumedComplexity = presumedComplexity;

	return result;
}

void ImageProcessor::SetCalculationParams(ComplexityCalculationParams parameters)
{
	_multiplier = parameters.Multiplier;
	_power = parameters.Power;
	_gainFactor = parameters.GainFactor;
	_smallDetailGainFactor = parameters.SmallDetailGainFactor;
	_performanceRate = parameters.PerformanceRate;
}

void ImageProcessor::SetDebugParams(const bool enableDebug, const char* debugPath)
{
	_enableDebug = enableDebug;
	_debugPath = std::string(debugPath);
}

const cv::Mat ImageProcessor::GetGrayscaleImage(const ColorImage& image)
{
	const int cvChannelsCode = ImageUtils::GetCvChannelsCodeFromBytesPerPixel(image.BytesPerPixel);
	cv::Mat cvImage(image.Height, image.Width, cvChannelsCode, image.Data);
	if (cvChannelsCode == CV_8UC4) // TODO: create a buffer to hold the BGR image
		cv::cvtColor(cvImage, cvImage, cv::COLOR_BGRA2BGR);

	cv::cvtColor(cvImage, cvImage, cv::COLOR_BGR2GRAY);

	return cvImage;
}

const Contour ImageProcessor::GetLargestContour(const std::vector<Contour>& contours) const
{
	if (contours.size() == 0)
		return Contour();

	if (contours.size() == 1)
		return contours[0];

	double largestArea = 0;
	Contour largestContour = Contour();

	for (uint i = 0; i < contours.size(); i++)
	{
		double contourArea = cv::contourArea(contours[i]);
		if (contourArea <= 0 || contourArea <= largestArea)
			continue;

		largestArea = contourArea;
		largestContour = contours[i];
	}

	return largestContour;
}

const Contour ImageProcessor::GetTargetContourFromImage(const cv::Mat& image) const
{
	const bool imageIsValid = image.cols > 0 && image.rows > 0 && image.data != nullptr;
	if (!imageIsValid)
		return Contour();

	cv::Mat cannied;
	cv::Canny(image, cannied, _cannyThreshold1, _cannyThreshold2);

	std::vector<Contour> contours;
	cv::findContours(cannied, contours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);

	Contour mergedContour;
	for (int i = 0; i < contours.size(); i++)
	{
		for (int j = 0; j < contours[i].size(); j++)
			mergedContour.emplace_back(contours[i][j]);
	}

	const int contourArea = !mergedContour.empty() ? (int)cv::contourArea(mergedContour) : 0;
	const bool contourExists = contourArea > 3;
	if (!contourExists)
		return Contour();

	return mergedContour;
}

const float ImageProcessor::GetCalculatedComplexity(const cv::Mat& image, const Contour& objectContour,
	const cv::RotatedRect& minBoundingRect,	const std::string& debugFileName)
{
	cv::Rect upRightBoundingRect = minBoundingRect.boundingRect();

	const int pointCount = 4;
	cv::Point2f minRectPoints[4];
	minBoundingRect.points(minRectPoints);
	Contour minRectContour;
	minRectContour.reserve(4);
	for (int i = 0; i < pointCount; i++)
		minRectContour.emplace_back(minRectPoints[i]);

	if (_enableDebug && _debugPath != "" && debugFileName != "")
		DrawDebugData(image, objectContour, minRectContour, debugFileName);

	int numOfEmptyPixels = 0;
	int numOfValuePixels = 0;

	for (int j = upRightBoundingRect.y; j < upRightBoundingRect.y + upRightBoundingRect.height; j++)
	{
		for (int i = upRightBoundingRect.x; i < upRightBoundingRect.x + upRightBoundingRect.width; i++)
		{
			const byte pixelValue = image.data[j * image.cols + i];
			const bool valueIsInContour = cv::pointPolygonTest(minRectContour, cv::Point(i, j), false) >= 0.0;
			if (!valueIsInContour)
				continue;

			if (pixelValue > 125)
				numOfEmptyPixels++;
			else
				numOfValuePixels++;
		}
	}

	const int totalNumOfMinRectPixels = numOfValuePixels + numOfEmptyPixels;
	if (totalNumOfMinRectPixels == 0)
		return -1;

	return (float)numOfValuePixels / totalNumOfMinRectPixels;
}

const float ImageProcessor::GetPresumedComplexity(const int partWidth, const int partHeight)
{
	const float minDim = (float)(partWidth < partHeight ? partWidth : partHeight);
	const float maxDim = (float)(partWidth > partHeight ? partWidth : partHeight);

	return _multiplier * (float)std::pow(minDim / maxDim, _power);
}

const float ImageProcessor::GetTotalComplexity(const float calculatedComplexity, const float presumedComplexity)
{
	if (calculatedComplexity <= presumedComplexity)
		return 1;
	
	return std::pow(calculatedComplexity - presumedComplexity + 1, _gainFactor);
}

const float ImageProcessor::GetLaborIntensity(const int partArea, const float totalComplexity)
{
	const float partAreaM2 = (float)partArea / (float)std::pow(1000, 2);

	return (float)std::pow(partAreaM2, 1 / _smallDetailGainFactor) * totalComplexity * _performanceRate;
}

void ImageProcessor::DrawDebugData(const cv::Mat& inputImage, const Contour& objectContour, const Contour& rotatedRectContour, 
	const std::string& debugFileName)
{
	auto matForRect = cv::Mat(inputImage);
	cv::cvtColor(matForRect, matForRect, cv::COLOR_GRAY2BGR);
	ImageUtils::DrawContour(matForRect, rotatedRectContour, cv::Scalar(0, 255, 0));
	const std::string& rectFilename = _debugPath + "/" + debugFileName + "_rect.png";
	cv::imwrite(rectFilename, matForRect);
}