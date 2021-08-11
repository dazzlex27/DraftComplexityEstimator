#pragma once

#include <algorithm>
#include "OpenCVInclude.h"

typedef unsigned char byte;
typedef unsigned int uint;

enum ComplexityCalculationStatus
{
	Undefined = 0,
	Success = 1,
	ImageError = 2,
	NoObjectFound = 3,
	CalculationError = 4
};

struct ColorImage
{
	int Width;
	int Height;
	byte* Data;
	byte BytesPerPixel;
};

struct ComplexityCalculationParams
{
	float Multiplier;
	float Power;
	float GainFactor;
	float SmallDetailGainFactor;
	float PerformanceRate;
};

struct ComplexityCalculationData
{
	const ColorImage* ColorImage;
	const int PartWidth;
	const int PartHeight;
	const char DebugFileName[128];
};

struct ComplexityCalculationResult
{
	ComplexityCalculationStatus Status;
	int LaborIntensityMinutes;
	float TotalComplexity;
	float CalculatedComplexity;
	float PresumedComplexity;
};