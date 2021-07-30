using System.Runtime.InteropServices;

namespace ImageProcessor.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ComplexityCalculationResult
	{
		public ComplexityCalculationStatus Status;
		public float Complexity;
		public int ContourWidth;
		public int ContourHeight;
	}
}
