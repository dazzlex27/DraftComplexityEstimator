#include "ImageUtils.h"
#include <cmath>
#include <climits>
#include <fstream>

const std::vector<Contour> ImageUtils::GetValidContours(const std::vector<Contour>& contours, const float minAreaRatio, 
	const int imageDataLength)
{
	std::vector<Contour> contoursValid;

	if (contours.size() == 0)
		return contoursValid;

	contoursValid.reserve(contours.size());
	const int minContourAreaPixels = (const int)(imageDataLength * minAreaRatio);
	for (int i = 0; i < contours.size(); i++)
	{
		if (contourArea(contours[i]) >= minContourAreaPixels)
			contoursValid.emplace_back(Contour(contours[i]));
	}

	return contoursValid;
}

const int ImageUtils::GetCvChannelsCodeFromBytesPerPixel(const int bytesPerPixel)
{
	switch (bytesPerPixel)
	{
	case 1:
		return CV_8UC1;
	case 3:
		return CV_8UC3;
	case 4:
		return CV_8UC4;
	default:
		return 0;
	}
}

cv::Mat ImageUtils::CreateEmptyMat(const int width, const int height, const int colorCode)
{
	return cv::Mat::zeros(height, width, colorCode);
}

void ImageUtils::DrawContour(cv::Mat& image, const Contour& contour, const cv::Scalar& color)
{
	std::vector<Contour> contoursToDraw;
	contoursToDraw.emplace_back(contour);

	cv::drawContours(image, contoursToDraw, 0, color);
}

// Source:  https://wrf.ecse.rpi.edu//Research/Short_Notes/pnpoly.html
bool ImageUtils::IsPointInsidePolygon(const std::vector<cv::Point>& polygon, int x, int y)
{
	bool pointIsInPolygon = false;

	for (int i = 0, j = (int)(polygon.size() - 1); i < (int)polygon.size(); j = i++)
	{
		bool pointInLineScope = (polygon[i].y > y) != (polygon[j].y > y);

		int lineXHalf = (polygon[j].x - polygon[i].x) * (y - polygon[i].y) / (polygon[j].y - polygon[i].y);
		int lineXPosition = lineXHalf + polygon[i].x;
		bool pointInLeftHalfPlaneOfLine = x < lineXPosition;

		if (pointInLineScope && pointInLeftHalfPlaneOfLine)
			pointIsInPolygon = !pointIsInPolygon;
	}

	return pointIsInPolygon;
}