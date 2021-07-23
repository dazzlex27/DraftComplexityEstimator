#include "ImageProcessingLibAPI.h"
#include "ImageProcessor.h"

ImageProcessor* Processor = nullptr;

DLL_EXPORT void CreateImageProcessor()
{
	if (Processor != nullptr)
		return;

	Processor = new ImageProcessor();
}

DLL_EXPORT void DestroyImageProcessor()
{
	if (Processor == nullptr)
		return;

	delete Processor;
	Processor = nullptr;
}

DLL_EXPORT void SetDebugPath(const char* path)
{
	Processor->SetDebugPath(path);
}

DLL_EXPORT ComplexityCalculationResult* CalculateObjectComplexity(ComplexityCalculationData calculationData)
{
	return Processor->CalculateObjectComplexity(calculationData);
}

void DisposeCalculationResult(ComplexityCalculationResult* result)
{
	if (result)
	{
		delete result;
		result = nullptr;
	}
}