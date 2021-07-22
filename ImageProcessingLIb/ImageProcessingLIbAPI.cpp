#include "ImageProcessingLibAPI.h"
#include "ImageProcessor.h"

ImageProcessor* Processor = nullptr;

DLL_EXPORT void CreateImageProcessor()
{
	if (Processor != nullptr)
		return;

	Processor = new ImageProcessor();
}

DLL_EXPORT void SetDebugPath(const char* path)
{
	Processor->SetDebugPath(path);
}

DLL_EXPORT ComplexityCalculationResult* CalculateDraftComplexity(ComplexityCalculationData calculationData)
{
	return Processor->CalculateDraftComplexity(calculationData);
}

void DisposeCalculationResult(ComplexityCalculationResult* result)
{
	if (result)
	{
		delete result;
		result = nullptr;
	}
}

DLL_EXPORT void DestroyImageProcessor()
{
	if (Processor)
	{
		delete Processor;
		Processor = nullptr;
	}
}