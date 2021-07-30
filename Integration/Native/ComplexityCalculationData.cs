using System.Runtime.InteropServices;

namespace ImageProcessor.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ComplexityCalculationData
	{
		public ColorImage* ColorImage;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string DebugFileName;
	}
}