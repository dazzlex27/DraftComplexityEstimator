namespace Integration
{
	internal class CalculationSample
	{
		public ImageData Image { get; }

		public float Complexity { get; } // [0-1]

		public CalculationSample(ImageData image, float complexity)
		{
			Image = image;
			Complexity = complexity;
		}
	}
}
