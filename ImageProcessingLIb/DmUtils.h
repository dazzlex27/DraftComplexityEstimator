#pragma once

#include "Structures.h"
#include "OpenCVInclude.h"

class DmUtils
{
public:
	static const std::vector<Contour> GetValidContours(const std::vector<Contour>& contours, const float minAreaRatio, const int imageDataLength);
	static const int GetCvChannelsCodeFromBytesPerPixel(const int bytesPerPixel);
	static const int GetCvGrayScaleConversionCode(const int cvColorCode);
	static void DrawTargetContour(const Contour& contour, const int width, const int height, const std::string& filename);
	static bool IsPointInsidePolygon(const std::vector<cv::Point>& polygon, int x, int y);
};