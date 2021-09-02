#pragma once

#include "Structures.h"
#include "OpenCVInclude.h"

class ImageUtils
{
public:
	static const std::vector<Contour> GetValidContours(const std::vector<Contour>& contours, const float minAreaRatio, const int imageDataLength);
	static const int GetCvChannelsCodeFromBytesPerPixel(const int bytesPerPixel);
	static cv::Mat CreateEmptyMat(const int width, const int height, const int colorCode);
	static void DrawContour(cv::Mat& image, const Contour& contour, const cv::Scalar& color);
	static bool IsPointInsidePolygon(const std::vector<cv::Point>& polygon, int x, int y);
	static cv::Mat RemoveAlphaChannel(const cv::Mat& image, const cv::Scalar& backgroundColor);
};