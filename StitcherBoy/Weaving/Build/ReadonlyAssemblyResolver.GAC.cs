// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

namespace StitcherBoy.Weaving.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using dnlib.DotNet;

    partial class ReadonlyAssemblyResolver
    {
        private class GacInfo
        {
            public readonly int Version;
            public readonly string Path;
            public readonly string Prefix;
            public readonly IList<string> SubDirs;

            public GacInfo(int version, string prefix, string path, IList<string> subDirs)
            {
                Version = version;
                Prefix = prefix;
                Path = path;
                SubDirs = subDirs;
            }
        }

        private static readonly string[] MonoVerDirs = new string[] {
			// The "-api" dirs are reference assembly dirs.
			"4.5", @"4.5\Facades", "4.5-api", @"4.5-api\Facades", "4.0", "4.0-api",
            "3.5", "3.5-api", "3.0", "3.0-api", "2.0", "2.0-api",
            "1.1", "1.0",
        };
        private static string[] _extraMonoPaths;
        private IList<GacInfo> _gacInfos;

        private static IEnumerable<string> FindMonoPrefixes()
        {
            yield return GetCurrentMonoPrefix();

            var prefixes = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
            if (!string.IsNullOrEmpty(prefixes))
            {
                foreach (var prefix in prefixes.Split(Path.PathSeparator))
                {
                    if (prefix != string.Empty)
                        yield return prefix;
                }
            }
        }

        private static string GetCurrentMonoPrefix()
        {
            var path = typeof(object).Module.FullyQualifiedName;
            for (int i = 0; i < 4; i++)
                path = Path.GetDirectoryName(path);
            return path;
        }

        private IEnumerable<string> FindAssembliesGacExactly(IAssembly assembly, ModuleDef sourceModule)
        {
            foreach (var gacInfo in GetGacInfos(sourceModule))
            {
                foreach (var path in FindAssembliesGacExactly(gacInfo, assembly))
                    yield return path;
            }
            if (_extraMonoPaths != null)
            {
                foreach (var path in GetExtraMonoPaths(assembly))
                    yield return path;
            }
        }

        static IEnumerable<string> FindAssembliesGacExactly(GacInfo gacInfo, IAssembly assembly)
        {
            var pkt = PublicKeyBase.ToPublicKeyToken(assembly.PublicKeyOrToken);
            if (gacInfo != null && pkt != null)
            {
                string pktString = pkt.ToString();
                string verString = CreateVersionWithNoUndefinedValues(assembly.Version).ToString();
                var cultureString = UTF8String.ToSystemStringOrEmpty(assembly.Culture);
                if (cultureString.Equals("neutral", StringComparison.OrdinalIgnoreCase))
                    cultureString = string.Empty;
                var asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
                foreach (var subDir in gacInfo.SubDirs)
                {
                    var baseDir = Path.Combine(gacInfo.Path, subDir);
                    baseDir = Path.Combine(baseDir, asmSimpleName);
                    baseDir = Path.Combine(baseDir, $"{gacInfo.Prefix}{verString}_{cultureString}_{pktString}");
                    var pathName = Path.Combine(baseDir, asmSimpleName + ".dll");
                    if (File.Exists(pathName))
                        yield return pathName;
                }
            }
        }

        private static Version CreateVersionWithNoUndefinedValues(Version a)
        {
            if (a == null)
                return new Version(0, 0, 0, 0);
            return new Version(a.Major, a.Minor, GetDefaultVersionValue(a.Build), GetDefaultVersionValue(a.Revision));
        }

        private static int GetDefaultVersionValue(int val)
        {
            return val == -1 ? 0 : val;
        }

        private IEnumerable<GacInfo> GetGacInfos(ModuleDef sourceModule)
        {
            if (_gacInfos != null)
                _gacInfos = LoadGAC();

            int version = sourceModule == null ? int.MinValue : sourceModule.IsClr40 ? 4 : 2;
            // Try the correct GAC first (eg. GAC4 if it's a .NET 4 assembly)
            return _gacInfos.Where(g => g.Version == version)
                .Concat(_gacInfos.Where(g => g.Version != version));
        }
        
        private IList<GacInfo> LoadGAC()
        {
            var gacInfos = new List<GacInfo>();

            if (Type.GetType("Mono.Runtime") != null)
            {
                var dirs = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                var extraMonoPathsList = new List<string>();
                foreach (var prefix in FindMonoPrefixes())
                {
                    var dir = Path.Combine(Path.Combine(Path.Combine(prefix, "lib"), "mono"), "gac");
                    if (dirs.ContainsKey(dir))
                        continue;
                    dirs[dir] = true;

                    if (Directory.Exists(dir))
                        gacInfos.Add(new GacInfo(-1, "", Path.GetDirectoryName(dir), new[] { Path.GetFileName(dir) }));

                    dir = Path.GetDirectoryName(dir);
                    foreach (var verDir in MonoVerDirs)
                    {
                        var dir2 = dir;
                        foreach (var d in verDir.Split('\\'))
                            dir2 = Path.Combine(dir2, d);
                        if (Directory.Exists(dir2))
                            extraMonoPathsList.Add(dir2);
                    }
                }

                var paths = Environment.GetEnvironmentVariable("MONO_PATH");
                if (paths != null)
                {
                    foreach (var path in paths.Split(Path.PathSeparator))
                    {
                        if (path != string.Empty && Directory.Exists(path))
                            extraMonoPathsList.Add(path);
                    }
                }
                _extraMonoPaths = extraMonoPathsList.ToArray();
            }
            else
            {
                var windir = Environment.GetEnvironmentVariable("WINDIR");
                if (!string.IsNullOrEmpty(windir))
                {
                    // .NET 1.x and 2.x
                    var path = Path.Combine(windir, "assembly");
                    if (Directory.Exists(path))
                        gacInfos.Add(new GacInfo(2, "", path, new[] { "GAC_32", "GAC_64", "GAC_MSIL", "GAC" }));

                    // .NET 4.x
                    path = Path.Combine(Path.Combine(windir, "Microsoft.NET"), "assembly");
                    if (Directory.Exists(path))
                        gacInfos.Add(new GacInfo(4, "v4.0_", path, new[] { "GAC_32", "GAC_64", "GAC_MSIL" }));
                }
            }
            return gacInfos;
        }

        private static IEnumerable<string> GetExtraMonoPaths(IAssembly assembly)
        {
            if (_extraMonoPaths != null)
            {
                foreach (var dir in _extraMonoPaths)
                {
                    var file = Path.Combine(dir, assembly.Name + ".dll");
                    if (File.Exists(file))
                        yield return file;
                }
            }
        }
    }
}
