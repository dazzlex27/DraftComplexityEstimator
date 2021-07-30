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
	_imageWidth = 0;
	_imageHeight = 0;
	_imageLengthBytes = 0;
	_imageBytesPerPixel = 0;
	_colorImageBuffer = nullptr;
}

ImageProcessor::~ImageProcessor()
{
	if (_colorImageBuffer != nullptr)
	{
		delete[] _colorImageBuffer;
		_colorImageBuffer = nullptr;
	}
}

ComplexityCalculationResult* ImageProcessor::CalculateObjectComplexity(const ComplexityCalculationData& data)
{
	auto result = new ComplexityCalculationResult();
	result->Complexity = -1;
	result->Status = ComplexityCalculationStatus::Undefined;

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
	const float complexity = CalculateComplexity(paddedImage, objectContour, minBoundingRect, std::string(data.DebugFileName));

	if (complexity < 0)
	{
		result->Status = ComplexityCalculationStatus::CalculationError;
		return result;
	}

	result->Status = ComplexityCalculationStatus::Success;
	result->Complexity = complexity;
	result->ContourWidth = (int)std::round(minBoundingRect.size.width);
	result->ContourHeight = (int)std::round(minBoundingRect.size.height);

	return result;
}

void ImageProcessor::SetDebugPath(const char* path)
{
	_debugPath = std::string(path);
}

const cv::Mat ImageProcessor::GetGrayscaleImage(const ColorImage& image)
{
	FillColorBufferFromImage(image);
	const int cvChannelsCode = ImageUtils::GetCvChannelsCodeFromBytesPerPixel(_imageBytesPerPixel);
	cv::Mat cvImage(_imageHeight, _imageWidth, cvChannelsCode, _colorImageBuffer);
	if (cvChannelsCode == CV_8UC4)
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

void ImageProcessor::FillColorBufferFromImage(const ColorImage& image)
{
	const bool dimsAreTheSame = _imageWidth == image.Width && _imageHeight == image.Height && 
		_imageLengthBytes == image.BytesPerPixel;
	if (!dimsAreTheSame)
	{
		_imageWidth = image.Width;
		_imageHeight = image.Height;
		const int colorImageLength = image.Width * image.Height;
		_imageLengthBytes = colorImageLength * image.BytesPerPixel;
		_imageBytesPerPixel = image.BytesPerPixel;

		if (_colorImageBuffer != nullptr)
			delete[] _colorImageBuffer;
		_colorImageBuffer = new byte[_imageLengthBytes];
	}

	memcpy(_colorImageBuffer, image.Data, _imageLengthBytes);
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

const float ImageProcessor::CalculateComplexity(const cv::Mat& image, const Contour& objectContour,
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

	if (_debugPath != "" && debugFileName != "")
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

void ImageProcessor::DrawDebugData(const cv::Mat& inputImage, const Contour& objectContour, const Contour& rotatedRectContour, 
	const std::string& debugFileName)
{
	auto matForRect = cv::Mat(inputImage);
	cv::cvtColor(matForRect, matForRect, cv::COLOR_GRAY2BGR);
	ImageUtils::DrawContour(matForRect, rotatedRectContour, cv::Scalar(0, 255, 0));
	const std::string& rectFilename = _debugPath + "/" + debugFileName + "_rect.png";
	cv::imwrite(rectFilename, matForRect);
}