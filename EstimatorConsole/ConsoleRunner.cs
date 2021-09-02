using System;
using System.Threading.Tasks;
using ImageProcessor;
using Primitives;

namespace EstimatorConsole
{
	internal class ConsoleRunner
	{
		private static readonly string ConfigPath = GlobalConstants.ConfigFilePath;
		
		private CalculationSettings _settings;
		private FileProcessor _fileProcessor;
		
		public async Task<int> Run(string inputFilePath)
		{
			try
			{
				Console.WriteLine("Loading data...");
				await LoadDataAsync();
			}
			catch (Exception)
			{
				return 3;
			}

			try
			{
				Console.WriteLine($"Processing file {inputFilePath}...");
				_fileProcessor.ProcessXlsx(inputFilePath);
			}
			catch (Exception)
			{
				return 4;
			}

			return 0;
		}
		
		private async Task LoadDataAsync()
		{
			await InitializeSettings();
			
			_fileProcessor = new FileProcessor();
			_fileProcessor.SetCalculationSettings(_settings);
		}

		private async Task InitializeSettings()
		{
			var settingsFromFile = await IoUtils.DeserializeSettingsFromFile<CalculationSettings>(ConfigPath);
			if (settingsFromFile == null)
			{
				_settings = CalculationSettings.GetDefaultSettings();
				await IoUtils.SerializeSettingsToFile(_settings, ConfigPath);
			}
			else
				_settings = settingsFromFile;
		}
	}
}