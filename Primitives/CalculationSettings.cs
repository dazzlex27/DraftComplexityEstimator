namespace Primitives
{
	public class CalculationSettings
	{
		public float Multiplier { get; set; }

		public float Power { get; set; }

		public float GainFactor { get; set; }

		public float SmallDetailGainFactor { get; set; }

		public float PerformanceRate { get; set; }

		public bool EnableDebug { get; set; }

		public string DebugDirectoryPath { get; set; }

		public CalculationSettings(float multiplier, float power, float gainFactor, float smallDetailGainFactor, float performanceRate,
			bool enableDebug, string debugDirectoryPath)
		{
			Multiplier = multiplier;
			Power = power;
			GainFactor = gainFactor;
			SmallDetailGainFactor = smallDetailGainFactor;
			PerformanceRate = performanceRate;
			EnableDebug = enableDebug;
			DebugDirectoryPath = debugDirectoryPath;
		}

		public static CalculationSettings GetDefaultSettings()
		{
			var defaultMultiplier = 0.0388f;
			var defaultPower = -0.686455f;
			var defaultGainFactor = 15.0f;
			var defaultSmallDetailGainFactor = 2.0f;
			var defaultPerformanceRate = 0.65f;
			var defaultDebugState = true;
			var defaultDebugDirectoryPath = GlobalConstants.DefaultDebugDirectory;

			return new CalculationSettings(defaultMultiplier, defaultPower, defaultGainFactor,
				defaultSmallDetailGainFactor, defaultPerformanceRate, defaultDebugState, defaultDebugDirectoryPath);
		}
	}
}
