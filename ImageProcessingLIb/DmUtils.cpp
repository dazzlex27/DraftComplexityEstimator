#include "DmUtils.h"
#include <cmath>
#include <climits>
#include <fstream>

const std::vector<Contour> DmUtils::GetValidContours(const std::vector<Contour>& contours, const float minAreaRatio, 
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

const int DmUtils::GetCvChannelsCodeFromBytesPerPixel(const int bytesPerPixel)
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

const int DmUtils::GetCvGrayScaleConversionCode(const int cvColorCode)
{
	switch (cvColorCode)
	{
	case 3:
		return cv::COLOR_BGR2GRAY;
	case 4:
		return cv::COLOR_BGRA2GRAY;
	default:
		return 0;
	}
}

void DmUtils::DrawTargetContour(const Contour& contour, const int width, const int height, const std::string& filename)
{
	cv::RotatedRect rect = cv::minAreaRect(cv::Mat(contour));
	cv::Point2f points[4];
	rect.points(points);

	Contour rectContour;
	rectContour.emplace_back(points[0]);
	rectContour.emplace_back(points[1]);
	rectContour.emplace_back(points[2]);
	rectContour.emplace_back(points[3]);

	std::vector<Contour> contoursToDraw;
	contoursToDraw.emplace_back(contour);
	contoursToDraw.emplace_back(rectContour);

	cv::Mat img2 = cv::Mat::zeros(height, width, CV_8UC3);

	cv::Scalar colors[3];
	colors[0] = cv::Scalar(255, 0, 0);
	colors[1] = cv::Scalar(0, 255, 0);
	for (auto i = 0; i < contoursToDraw.size(); i++)
		cv::drawContours(img2, contoursToDraw, i, colors[i]);

	cv::imwrite(filename, img2);
}

// Source:  https://wrf.ecse.rpi.edu//Research/Short_Notes/pnpoly.html
bool DmUtils::IsPointInsidePolygon(const std::vector<cv::Point>& polygon, int x, int y)
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