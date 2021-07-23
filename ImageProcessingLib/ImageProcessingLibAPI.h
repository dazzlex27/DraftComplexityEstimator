#pragma once

#include "Structures.h"

#define DLL_EXPORT extern "C" _declspec(dllexport)

DLL_EXPORT void CreateImageProcessor();
DLL_EXPORT void DestroyImageProcessor();
DLL_EXPORT void SetDebugPath(const char* path);
DLL_EXPORT ComplexityCalculationResult* CalculateObjectComplexity(ComplexityCalculationData data);
DLL_EXPORT void DisposeCalculationResult(ComplexityCalculationResult* result);