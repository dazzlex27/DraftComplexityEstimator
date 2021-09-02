using System;
using System.IO;
using System.Diagnostics;

namespace Primitives
{
	public static class GlobalConstants
	{
		public const string ProcessedFileSuffix = "_processed";
		
		public static readonly string AppConfigPath;
		public static readonly string ConfigFilePath;
		public static readonly string DefaultDebugDirectory;

		static GlobalConstants()
		{
			var myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var appName = Process.GetCurrentProcess().ProcessName;
			AppConfigPath = Path.Combine(myDocumentsPath, appName);
			ConfigFilePath = Path.Combine(AppConfigPath, "settings.cfg");
			DefaultDebugDirectory = Path.Combine(AppConfigPath, "output");
		}
	}
}