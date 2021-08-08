using System.Runtime.InteropServices;

namespace ImageProcessor.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ComplexityCalculationParams
	{
		public float Multiplier;
		public float Power;
		public float GainFactor;
		public float SmallDetailGainFactor;
		public float PerformanceRate;
	};
}