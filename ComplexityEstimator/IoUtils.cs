using Newtonsoft.Json;
using Primitives;
using System.IO;

namespace ComplexityEstimator
{
	internal static class IoUtils
	{
		public static void SerializeSettings(CalculationSettings settings)
		{
			Directory.CreateDirectory(GlobalConstants.AppConfigPath);
			if (settings == null)
				return;

			var settingsText = JsonConvert.SerializeObject(settings);
			File.WriteAllText(GlobalConstants.ConfigFilePath, settingsText);
		}

		public static CalculationSettings DeserializeSettings()
		{
			Directory.CreateDirectory(GlobalConstants.AppConfigPath);
			if (!File.Exists(GlobalConstants.ConfigFilePath))
				return null;

			var settingsText = File.ReadAllText(GlobalConstants.ConfigFilePath);

			return JsonConvert.DeserializeObject<CalculationSettings>(settingsText);
		}
	}
}