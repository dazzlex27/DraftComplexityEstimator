using System.Runtime.InteropServices;

namespace ImageProcessor.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ColorImage
	{
		public int Width;
		public int Height;
		public byte* Data;
		public byte BytesPerPixel;
	}
}
