namespace ImageProcessor
{
	internal class ComplexityInfo
	{
		public float Complexity { get; }

		public int ContourWidth { get; }

		public int ContourHeight { get; }

		public ComplexityInfo(float complexity, int contourWidth, int contourHeight)
		{
			Complexity = complexity;
			ContourWidth = contourWidth;
			ContourHeight = contourHeight;
		}
	}
}