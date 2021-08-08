using System.Runtime.InteropServices;

namespace ImageProcessor.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ComplexityCalculationResult
	{
		public ComplexityCalculationStatus Status;
		public float LaborIntensity;
		public float TotalComplexity;
		public float CalculatedComplexity;
		public float PresumedComplexity;
	}
}
