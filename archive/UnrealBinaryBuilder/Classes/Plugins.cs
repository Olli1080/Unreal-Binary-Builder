using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace UnrealBinaryBuilder.Classes
{
	public class Plugins
	{
        public static List<EngineBuild>? GetInstalledEpicEngines()
        {
            RegistryKey? EngineInstallations = Registry.LocalMachine.OpenSubKey(@"Software\EpicGames\Unreal Engine");
            if (EngineInstallations == null) return null;

            List<EngineBuild> ReturnValue = [];
            string[] InstalledEngines = EngineInstallations.GetSubKeyNames();
            foreach (var s in InstalledEngines)
            {
                RegistryKey? InstalledDirectoryKey = EngineInstallations.OpenSubKey(s);

                object? o = InstalledDirectoryKey?.GetValue("InstalledDirectory");

                if (o is not string EnginePath) continue;
                if (!Directory.Exists(EnginePath)) continue;

                EngineBuild engineBuild = new EngineBuild
                {
                    bIsCustomEngine = false,
                    EngineAssociation = s,
                    EngineName = s,
                    EnginePath = EnginePath
                };

                ReturnValue.Add(engineBuild);
            }
			return ReturnValue;
        }

        public static List<EngineBuild>? GetInstalledCustomEngines()
        {
            RegistryKey? CustomEngineInstallations = Registry.CurrentUser.OpenSubKey(@"Software\Epic Games\Unreal Engine\Builds");

            if (CustomEngineInstallations == null) return null;

            List<EngineBuild> ReturnValue = [];
            string[] InstalledEngines = CustomEngineInstallations.GetValueNames();
            foreach (var s in InstalledEngines)
            {
                object? o = CustomEngineInstallations.GetValue(s);

                if (o is not string BaseEnginePath) continue;
                if (!Directory.Exists(BaseEnginePath)) continue;

                string? EngineBuildName = UnrealBinaryBuilderHelpers.GetEngineVersion(BaseEnginePath);
                if (EngineBuildName == null) continue;

                EngineBuild engineBuild = new EngineBuild
                {
                    bIsCustomEngine = true,
                    EngineAssociation = s,
                    EngineName = $"{EngineBuildName} (Custom) {s}",
                    EnginePath = BaseEnginePath
                };
                ReturnValue.Add(engineBuild);
            }
            return ReturnValue;
        }

		public static List<EngineBuild> GetInstalledEngines()
        {
            return [..GetInstalledEpicEngines() ?? [], ..GetInstalledCustomEngines() ?? []];
        }
	}

	class UE4PluginJson
	{
		public string FriendlyName { get; set; }
		public string Description { get; set; }
		public string MarketplaceURL { get; set; }
		public bool IsBetaVersion { get; set; }
		public bool IsExperimentalVersion { get; set; }
		public IList<UE4PluginModule> Modules { get; set; }
	}

	class UE4PluginModule
	{
		public IList<string> WhitelistPlatforms { get; set; }
	}

	public class EngineBuild
	{
		public string EngineName { get; set; }
		public string EnginePath { get; set; }
		public bool bIsCustomEngine { get; set; }
		public string EngineAssociation { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is EngineBuild build)
			{
				return build.EngineName == EngineName;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return EngineName.GetHashCode();
		}
	}
}
