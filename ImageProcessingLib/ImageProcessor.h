#pragma once

#include "Structures.h"
#include "OpenCVInclude.h"

class ImageProcessor
{
private:
	const int _cannyThreshold1 = 50;
	const int _cannyThreshold2 = 200;
	std::string _debugPath;

	int _imageWidth;
	int _imageHeight;
	int _imageBytesPerPixel;
	int _imageLengthBytes;
	byte* _colorImageBuffer;

public:
	ImageProcessor();
	~ImageProcessor();

	ComplexityCalculationResult* CalculateObjectComplexity(const ComplexityCalculationData& data);
	void SetDebugPath(const char* path);

private:
	const cv::Mat GetCvImage(const ColorImage& image);
	const Contour GetLargestContour(const std::vector<Contour>& contours) const;
	void FillColorBufferFromImage(const ColorImage& image);
	const Contour GetTargetContourFromImage(const cv::Mat& image) const;
	const float CalculateComplexity(const cv::Mat& image, const Contour& colorObjectContour);
};