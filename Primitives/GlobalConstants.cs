using System;
using System.IO;
using System.Diagnostics;

namespace Primitives
{
	public class GlobalConstants
	{
		public static readonly string AppConfigPath;
		public static readonly string ConfigFilePath;
		public static readonly string DefaultDebugDirectory;

		private static readonly string MyDocumentsPath;
		private static readonly string AppName;

		static GlobalConstants()
		{
			MyDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			AppName = Process.GetCurrentProcess().ProcessName;
			AppConfigPath = Path.Combine(MyDocumentsPath, AppName);
			ConfigFilePath = Path.Combine(AppConfigPath, "settings.cfg");
			DefaultDebugDirectory = Path.Combine(AppConfigPath, "output");
		}
	}
}