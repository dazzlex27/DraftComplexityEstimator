#include "ImageProcessor.h"
#include <string>
#include <cmath>
#include <climits>
#include <cstdlib>
#include <ctime>
#include "DmUtils.h"
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

ComplexityCalculationResult* ImageProcessor::CalculateDraftComplexity(const ComplexityCalculationData& data)
{
	auto result = new ComplexityCalculationResult();
	result->Complexity = -1;
	result->Status = ComplexityCalculationStatus::Undefined;

	if (data.ColorImage == nullptr || data.ColorImage->Data == nullptr)
	{
		result->Status = ComplexityCalculationStatus::ImageError;
		return result;
	}

	const auto image = GetCvImage(*(data.ColorImage));

	const Contour& objectContour = GetTargetContourFromImage(image);
	if (!objectContour.size() == 0)
	{
		result->Status = ComplexityCalculationStatus::NoObjectFound;
		return result;
	}

	const std::string debugFileName(data.DebugFileName);
	if (_debugPath != "" && debugFileName != "")
	{
		const std::string& colorFilename = _debugPath + "/" + debugFileName + "_ctr.png";
		DmUtils::DrawTargetContour(objectContour, _imageWidth, _imageHeight, colorFilename);
	}

	const float complexity = CalculateComplexity(image, objectContour);

	if (complexity < 0)
	{
		result->Status = ComplexityCalculationStatus::CalculationError;
		return result;
	}

	result->Complexity = complexity;
	result->Status = ComplexityCalculationStatus::Success;

	return result;
}

void ImageProcessor::SetDebugPath(const char* path)
{
	_debugPath = std::string(path);
}

const cv::Mat ImageProcessor::GetCvImage(const ColorImage& image)
{
	FillColorBufferFromImage(image);
	const int cvChannelsCode = DmUtils::GetCvChannelsCodeFromBytesPerPixel(_imageBytesPerPixel);
	cv::Mat cvImage(_imageHeight, _imageWidth, cvChannelsCode, _colorImageBuffer);
	const int colorToGrayscaleConversionCode = DmUtils::GetCvGrayScaleConversionCode(cvChannelsCode);
	if (colorToGrayscaleConversionCode != 0)
		cv::cvtColor(cvImage, cvImage, colorToGrayscaleConversionCode);

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

const float ImageProcessor::CalculateComplexity(const cv::Mat& image, const Contour& colorObjectContour)
{
	const auto minBoundingRect = cv::minAreaRect(colorObjectContour);
	cv::Rect upRightBoundingRect = minBoundingRect.boundingRect();

	cv::Mat roiImage = image(upRightBoundingRect);

	cv::Point2f minBoundingRectPoints[4];
	minBoundingRect.points(minBoundingRectPoints);
	std::vector<cv::Point2f> minBoundingRectPointArray;
	minBoundingRectPointArray.reserve(4);
	minBoundingRectPointArray.emplace_back(minBoundingRectPoints[0]);
	minBoundingRectPointArray.emplace_back(minBoundingRectPoints[1]);
	minBoundingRectPointArray.emplace_back(minBoundingRectPoints[2]);
	minBoundingRectPointArray.emplace_back(minBoundingRectPoints[3]);

	int numOfEmptyPixels = 0;
	int numOfValuePixels = 0;

	for (int j = upRightBoundingRect.y; j < upRightBoundingRect.y + upRightBoundingRect.height; j++)
	{
		for (int i = upRightBoundingRect.x; i < upRightBoundingRect.x + upRightBoundingRect.width; i++)
		{
			const byte pixelValue = roiImage.data[j * roiImage.cols + i];
			const bool valueIsInContour = cv::pointPolygonTest(minBoundingRectPointArray, cv::Point(i, j), false) >= 0.0;
			if (!valueIsInContour)
				continue;

			if (pixelValue > 0)
				numOfValuePixels++;
			else
				numOfEmptyPixels++;
		}
	}

	const int totalNumOfMinRectPixels = numOfValuePixels + numOfEmptyPixels;
	if (totalNumOfMinRectPixels == 0)
		return -1;

	return (float)numOfValuePixels / totalNumOfMinRectPixels;
}