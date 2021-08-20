using System.Runtime.InteropServices;

namespace ImageProcessor.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal readonly struct ComplexityCalculationResult
	{
		public readonly ComplexityCalculationStatus Status;
		public readonly int LaborIntensityMinutes;
		public readonly float TotalComplexity;
		public readonly float CalculatedComplexity;
		public readonly float PresumedComplexity;
	}
}
