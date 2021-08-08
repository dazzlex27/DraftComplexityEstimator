using System.Windows;
using System.Windows.Input;
using Primitives;

namespace ComplexityEstimator
{
	internal class SettingsWindowVm : BaseViewModel
	{
		private float _multiplier;
		private float _power;
		private float _gainFactor;
		private float _smallDetailGainFactor;
		private float _performanceRate;
		private bool _enableDebug;
		private string _debugDirectoryPath;

		public float Multiplier
		{
			get => _multiplier;
			set => SetField(ref _multiplier, value, nameof(Multiplier));
		}

		public float Power
		{
			get => _power;
			set => SetField(ref _power, value, nameof(Power));
		}

		public float GainFactor
		{
			get => _gainFactor;
			set => SetField(ref _gainFactor, value, nameof(GainFactor));
		}

		public float SmallDetailGainFactor
		{
			get => _smallDetailGainFactor;
			set => SetField(ref _smallDetailGainFactor, value, nameof(SmallDetailGainFactor));
		}

		public float PerformanceRate
		{
			get => _performanceRate;
			set => SetField(ref _performanceRate, value, nameof(PerformanceRate));
		}

		public bool EnableDebug
		{
			get => _enableDebug;
			set => SetField(ref _enableDebug, value, nameof(EnableDebug));
		}

		public string DebugDirectoryPath
		{
			get => _debugDirectoryPath;
			set => SetField(ref _debugDirectoryPath, value, nameof(DebugDirectoryPath));
		}

		public ICommand ResetSettingsCommand { get; }

		public SettingsWindowVm(CalculationSettings settings)
		{
			ResetSettingsCommand = new CommandHandler(ResetSettings, true);

			FillValuesFromSettings(settings);
		}

		public CalculationSettings GetSettings()
		{
			return new CalculationSettings(Multiplier, Power, GainFactor, SmallDetailGainFactor, PerformanceRate,
				EnableDebug, DebugDirectoryPath);
		}

		private void ResetSettings()
		{
			if (MessageBox.Show("Сбросить настройки?", "Подтверждение", MessageBoxButton.YesNo,
					MessageBoxImage.Question) != MessageBoxResult.Yes)
				return;

			FillValuesFromSettings(CalculationSettings.GetDefaultSettings());
		}

		private void FillValuesFromSettings(CalculationSettings settings)
		{
			Multiplier = settings.Multiplier;
			Power = settings.Power;
			GainFactor = settings.GainFactor;
			SmallDetailGainFactor = settings.SmallDetailGainFactor;
			PerformanceRate = settings.PerformanceRate;
			EnableDebug = settings.EnableDebug;
			DebugDirectoryPath = settings.DebugDirectoryPath;
		}
	}
}