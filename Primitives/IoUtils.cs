using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Primitives
{
	public static class IoUtils
	{
		public static async Task SerializeSettingsToFile<T>(T settings, string filename)
		{
			var logFileInfo = new FileInfo(filename);
			if (!string.IsNullOrEmpty(logFileInfo.DirectoryName))
				Directory.CreateDirectory(logFileInfo.DirectoryName);
			
			if (settings == null)
				return;

			var settingsText = JsonConvert.SerializeObject(settings);
			await File.WriteAllTextAsync(filename, settingsText);
		}

		public static async Task<T> DeserializeSettingsFromFile<T>(string filename)
		{
			var configFileInfo = new FileInfo(filename);
			if (!string.IsNullOrEmpty(configFileInfo.DirectoryName))
				Directory.CreateDirectory(configFileInfo.DirectoryName);
			
			if (!configFileInfo.Exists)
				return default;

			var settingsText = await File.ReadAllTextAsync(configFileInfo.FullName);
			return JsonConvert.DeserializeObject<T>(settingsText);
		}
	}
}