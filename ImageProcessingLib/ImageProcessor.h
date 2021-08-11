#pragma once

#include "Structures.h"
#include "OpenCVInclude.h"

class ImageProcessor
{
private:
	const int _cannyThreshold1 = 50;
	const int _cannyThreshold2 = 200;

	float _multiplier;
	float _power;
	float _gainFactor;
	float _smallDetailGainFactor;
	float _performanceRate;

	bool _enableDebug;
	std::string _debugPath;

public:
	ImageProcessor();
	~ImageProcessor();

	ComplexityCalculationResult* CalculateObjectComplexity(const ComplexityCalculationData& data);
	void SetCalculationParams(ComplexityCalculationParams parameters);
	void SetDebugParams(const bool enableDebug, const char* debugPath);

private:
	const cv::Mat GetGrayscaleImage(const ColorImage& image);
	const Contour GetLargestContour(const std::vector<Contour>& contours) const;
	const Contour GetTargetContourFromImage(const cv::Mat& image) const;
	const float GetCalculatedComplexity(const cv::Mat& image, const Contour& colorObjectContour, const cv::RotatedRect& minBoundingRect,
		const std::string& debugFileName);
	const float GetPresumedComplexity(const int partWidth, const int partHeight);
	const float GetTotalComplexity(const float calculatedComplexity, const float presumedComplexity);
	const int GetLaborIntensityMinutes(const int partArea, const float totalComplexity);
	void DrawDebugData(const cv::Mat& inputImage, const Contour& objectContour, const Contour& rotatedRectContour,
		const std::string& debugFileName);
};