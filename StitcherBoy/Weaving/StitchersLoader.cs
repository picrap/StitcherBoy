namespace StitcherBoy.Weaving
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Project;

    internal class StitchersLoader
    {
        public IEnumerable<Stitcher> GetRetouchers(ProjectDefinition project, string projectPath, string solutionPath)
        {
            return GetNuGetClients(projectPath, solutionPath).Concat(GetReferencesClients(project));
        }

        /// <summary>
        /// Gets the clients from assembly references.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        private static IEnumerable<Stitcher> GetReferencesClients(ProjectDefinition project)
        {
            foreach (var reference in project.References.Where(r => !r.Gac))
            {
                var retoucher = GetRetoucher(reference.Assembly);
                if (retoucher != null)
                    yield return retoucher;
            }
        }

        /// <summary>
        /// Gets the clients from NuGet package.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns></returns>
        private static IEnumerable<Stitcher> GetNuGetClients(string projectPath, string solutionPath)
        {
            var packages = GetNuGetPackageIDs(projectPath).ToArray();
            foreach (var package in packages)
            {
                var packagePath = GetPackagePath(solutionPath, package);
                var clientsPaths = GetAssembliesPaths(packagePath);
                foreach (var clientPath in clientsPaths)
                {
                    var assembly = TryLoad(clientPath);
                    if (assembly != null)
                    {
                        var retoucher = GetRetoucher(assembly);
                        if (retoucher != null)
                            yield return retoucher;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the client from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        private static Stitcher GetRetoucher(Assembly assembly)
        {
            var clientTypes = assembly.GetTypes().Where(t => t.IsPublic && t.Name == "Stitcher");
            foreach (var clientType in clientTypes)
            {
                var entryMethod = clientType.GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
                if (entryMethod != null)
                    return new Stitcher(entryMethod);
            }
            return null;
        }

        /// <summary>
        /// Tries to load an <see cref="Assembly"/> from a given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static Assembly TryLoad(string path)
        {
            try
            {
                return Assembly.LoadFrom(path);
            }
            catch (FileLoadException)
            { }
            catch (BadImageFormatException)
            { }
            return null;
        }

        /// <summary>
        /// Tries to find assemblies in package path.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetAssembliesPaths(string packagePath)
        {
            var rootAssemblies = GetAssemblies(packagePath).ToArray();
            if (rootAssemblies.Any())
                return rootAssemblies;
            var toolsAssemblies = GetAssemblies(Path.Combine(packagePath, "Tools"));
            return toolsAssemblies;
        }

        /// <summary>
        /// Gets the assemblies paths from a given directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetAssemblies(string directory)
        {
            if (!Directory.Exists(directory))
                return new string[0];
            var assemblies = Directory.EnumerateFiles(directory, "*.dll")
                .Concat(Directory.EnumerateFiles(directory, "*.exe"));
            return assemblies;
        }

        /// <summary>
        /// Gets the path from a given package.
        /// </summary>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        private static string GetPackagePath(string solutionPath, NuGetPackage package)
        {
            var solutionDir = Path.GetDirectoryName(solutionPath);
            var packagesPath = Path.Combine(solutionDir, "packages");
            var packagePath = Path.Combine(packagesPath, $"{package.ID}.{package.Version}");
            return packagePath;
        }

        /// <summary>
        /// Gets the NuGet packages.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <returns></returns>
        private static IEnumerable<NuGetPackage> GetNuGetPackageIDs(string projectPath)
        {
            var packagesConfigPath = Path.Combine(Path.GetDirectoryName(projectPath), "packages.config");
            if (File.Exists(packagesConfigPath))
            {
                using (var textReader = File.OpenText(packagesConfigPath))
                {
                    var document = XDocument.Load(textReader);
                    var root = document.Root;
                    if (root != null)
                    {
                        return from element in root.Nodes().OfType<XElement>()
                               let idAttribute = element.Attributes().SingleOrDefault(a => a.Name == "id")
                               let versionAttribute = element.Attributes().SingleOrDefault(a => a.Name == "version")
                               let targetAttribute = element.Attributes().SingleOrDefault(a => a.Name == "targetFramework")
                               where idAttribute != null && versionAttribute != null && targetAttribute != null
                               select new NuGetPackage
                               {
                                   ID = idAttribute.Value,
                                   Version = versionAttribute.Value,
                                   TargetFramework = targetAttribute.Value
                               };
                    }
                }
            }
            return new NuGetPackage[0];
        }
    }
}
