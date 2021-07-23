using System.Runtime.InteropServices;

namespace ComplexityEstimator.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ComplexityCalculationData
	{
		public ColorImage* ColorImage;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string DebugFileName;
	}
}