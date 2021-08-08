namespace Primitives
{
	public class ComplexityInfo
	{
		public float LaborIntensity { get; }

		public float TotalComplexity { get; }

		public float CalculatedComplexity { get; }

		public float PresumedComplexity { get; }

		public ComplexityInfo(float laborIntensity, float totalComplexity, float calculatedComplexity, float presumedComplexity)
		{
			LaborIntensity = laborIntensity;
			TotalComplexity = totalComplexity;
			CalculatedComplexity = calculatedComplexity;
			PresumedComplexity = presumedComplexity;
		}
	}
}