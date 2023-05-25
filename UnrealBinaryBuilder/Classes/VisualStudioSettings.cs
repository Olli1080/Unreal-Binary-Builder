using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnrealBinaryBuilder.Classes
{
    public class VisualStudioMsBuild
    {
        public static VisualStudioMsBuild ParseMsBuild(string path)
        {
            VisualStudioMsBuild msBuild = new VisualStudioMsBuild
            {
                _type = Path.GetFileName(path)
            };

            string msBuildPath = Path.Combine(path, "MSBuild");
            if (Directory.Exists(Path.Combine(msBuildPath, "Current")))
                msBuildPath = Path.Combine(msBuildPath, "Current");
            /*else
            {
            }*/

            foreach (string exePath in Directory.GetFiles(msBuildPath, "msbuild.exe", SearchOption.AllDirectories))
            {
                string architecture = Path.GetFileName(Path.GetDirectoryName(exePath));
                if (architecture == "amd64")
                    msBuild._x64 = exePath;
                else if (architecture == "Bin")
                    msBuild._x32 = exePath;
            }

            if (msBuild._x64 != "" || msBuild._x32 != "")
                return msBuild;

            return null;
        }

        public string Type => _type;
        public string X64Path => _x64;
        public string X32Path => _x32;

        private string _type;
        private string _x64 = "";
        private string _x32 = "";
    }
    public class VisualStudioVersion
    {
        public static VisualStudioVersion ParseVersion(string path)
        {
            VisualStudioVersion version = new VisualStudioVersion
            {
                _version = int.Parse(Path.GetFileName(path))
            };

            foreach (var directory in Directory.GetDirectories(path))
            {
                var build = VisualStudioMsBuild.ParseMsBuild(directory);
                if (build != null)
                    version._msBuilds.Add(build);
            }

            if (version._msBuilds.Count != 0)
                return version;

            return null;
        }

        public int Version => _version;
        public List<VisualStudioMsBuild> MsBuilds => _msBuilds;

        private int _version;
        private List<VisualStudioMsBuild> _msBuilds = new();
    }

    public class VisualStudioConfigurations
    {
        public static string MSVC = "Microsoft Visual Studio";
        public static string GetX86()
        {
            return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        }
        public static string GetX64()
        {
            return Environment.GetEnvironmentVariable("ProgramW6432");
        }

        public VisualStudioConfigurations()
        {
            string x86Path = Path.Combine(GetX86(), MSVC);
            string x64Path = Path.Combine(GetX64(), MSVC);

            List<string> VisualStudioPaths = new List<string>();

            if (Directory.Exists(x86Path))
                VisualStudioPaths.Add(x86Path);

            if (Directory.Exists(x64Path))
                VisualStudioPaths.Add(x64Path);

            VisualStudioPaths.ForEach(topLevelDir =>
            {
                IEnumerable<string> potentialVersions = from dir in Directory.GetDirectories(topLevelDir)
                                                        where int.TryParse(Path.GetFileName(dir), out _)
                                                        select dir;

                foreach (var potentialVersion in potentialVersions)
                {
                    VisualStudioVersion version = VisualStudioVersion.ParseVersion(potentialVersion);
                    if (version != null)
                        _versions.Add(version);
                }
            });
        }

        public List<VisualStudioVersion> Versions => _versions;
        private List<VisualStudioVersion> _versions = new List<VisualStudioVersion>();
    }
}
