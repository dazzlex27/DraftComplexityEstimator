using System.Runtime.InteropServices;

namespace ImageProcessor.Native
{
	internal static class ImageProcessingDll
	{
		private const string AnalyzerLibName = "ImageProcessingLib.dll";

		[DllImport(AnalyzerLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void CreateImageProcessor();

		[DllImport(AnalyzerLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyImageProcessor();

		[DllImport(AnalyzerLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetCalculationParams(ComplexityCalculationParams parameters);

		[DllImport(AnalyzerLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetDebugParams(bool enableDebug, string debugPath);

		[DllImport(AnalyzerLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe ComplexityCalculationResult* CalculateObjectComplexity(ComplexityCalculationData data);

		[DllImport(AnalyzerLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void DisposeCalculationResult(ComplexityCalculationResult* result);
	}
}