using ImageProcessor;

namespace ImageProcessor
{
	internal class CalculationSample
	{
		public ImageData Image { get; }

		public ComplexityInfo ComplexityInfo { get; } // [0-1], int, int

		public CalculationSample(ImageData image, ComplexityInfo complexityInfo)
		{
			Image = image;
			ComplexityInfo = complexityInfo;
		}
	}
}
