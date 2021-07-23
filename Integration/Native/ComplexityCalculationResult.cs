using System.Runtime.InteropServices;

namespace ComplexityEstimator.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ComplexityCalculationResult
	{
		public float Complexity;
		public ComplexityCalculationStatus Status;
	}
}
