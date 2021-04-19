#region Arx One
// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using dnlib.DotNet;
    using dnlib.DotNet.Writer;
    using Logging;
    using Reflection;

    /// <summary>
    /// Assembly stitcher
    /// </summary>
    /// <seealso cref="StitcherBoy.Weaving.IStitcher" />
    public abstract class AssemblyStitcher : IStitcher
    {
        /// <summary>
        /// Occurs when module written.
        /// </summary>
        public event EventHandler ModuleWritten;

        /// <summary>
        /// Gets the name (to ensure assembly is stitched only once).
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the logging.
        /// </summary>
        /// <value>
        /// The logging.
        /// </value>
        public ILogging Logging { get; set; }

        /// <summary>
        /// Processes the assembly based on given parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="entryAssemblyPath">The entry assembly path.</param>
        /// <returns></returns>
        public bool Process(IDictionary<string, string> parameters,
            string entryAssemblyPath)
        {
            parameters.TryGetValue("AssemblyPath", out var assemblyPath);
            var assemblyMarkerPath = assemblyPath + ".❤" + Name;
            if (HasProcessed(assemblyPath, assemblyMarkerPath))
                return true;
            parameters.TryGetValue("Configuration", out var configuration);
            parameters.TryGetValue("SignAssembly", out var literalSignAssembly);
            parameters.TryGetValue("ReferencePath", out var referencePath);
            parameters.TryGetValue("ReferenceCopyLocalPaths", out var referenceCopyLocalPaths);
            bool signAssembly = false;
            if (literalSignAssembly is not null)
                bool.TryParse(literalSignAssembly, out signAssembly);
            parameters.TryGetValue("AssemblyOriginatorKeyFile", out var assemblyOriginatorKeyFile);
            if (!signAssembly)
                assemblyOriginatorKeyFile = null;

            bool success = true;
            using (var moduleHandler = LoadModule(assemblyPath))
            {
                bool ok;
                try
                {
                    var context = new AssemblyStitcherContext
                    {
                        Module = moduleHandler?.Module,
                        Dependencies = EnumerateDependencies(referencePath, referenceCopyLocalPaths).ToArray(),
                        AssemblyPath = assemblyPath,
                        TaskAssemblyPath = entryAssemblyPath,
                        Configuration = configuration,
                    };
                    context.AssemblyResolver = new ReadonlyAssemblyResolver(context.Dependencies.Select(d => d.Path));
                    ok = Process(context);
                }
                catch (Exception e)
                {
                    Logging.WriteError("Uncaught exception: {0}", e.ToString());
                    ok = false;
                    success = false;
                }

                if (ok)
                {
                    moduleHandler?.Write(assemblyOriginatorKeyFile);
                    SetProcessed(assemblyMarkerPath);
                }
            }

            var onModuleWritten = ModuleWritten;
            if (onModuleWritten is not null)
                onModuleWritten(this, EventArgs.Empty);

            return success;
        }

        private static void SetProcessed(string assemblyMarkerPath)
        {
            using var stream = File.Create(assemblyMarkerPath);
        }

        private static bool HasProcessed(string assemblyPath, string assemblyMarkerPath)
        {
            if (!File.Exists(assemblyMarkerPath))
                return false;
            var assemblyInfo = new FileInfo(assemblyPath);
            var assemblyMarkerInfo = new FileInfo(assemblyMarkerPath);
            return assemblyMarkerInfo.LastWriteTimeUtc > assemblyInfo.LastWriteTimeUtc;
        }

        /// <summary>
        /// Loads the module.
        /// This can be overriden to return null and avoid loading target module
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns></returns>
        protected virtual ModuleManager LoadModule(string assemblyPath)
        {
            // when assembly is not provided... Do not load it :)
            if (string.IsNullOrEmpty(assemblyPath))
                return null;
            return new ModuleManager(assemblyPath, true, true, logger: Logging);
        }

        private static IEnumerable<AssemblyDependency> EnumerateDependencies(string referencePath, string referenceCopyLocalPaths)
        {
            var referenceCopyLocalPathsList = GetList(referenceCopyLocalPaths).ToList();
            var referencePathList = GetList(referencePath).Where(s => !referenceCopyLocalPathsList.Any(r => string.Equals(r, s, StringComparison.InvariantCultureIgnoreCase))).ToList();
            return referenceCopyLocalPathsList.Select(p => new AssemblyDependency(p, true)).Concat(referencePathList.Select(p => new AssemblyDependency(p, false)));
        }

        private static IEnumerable<string> GetList(string paths)
        {
            if (paths is not null)
            {
                foreach (var path in paths.Split(';'))
                {
                    var extension = Path.GetExtension(path);
                    if (string.Equals(extension, ".exe", StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(extension, ".dll", StringComparison.InvariantCultureIgnoreCase))
                        yield return path;
                }
            }
        }

        private static TOptions SetWriterOptions<TOptions>(string snkPath, TOptions options)
            where TOptions : ModuleWriterOptionsBase
        {
            options.WritePdb = true;
            if (!string.IsNullOrEmpty(snkPath) && File.Exists(snkPath))
                options.StrongNameKey = new StrongNameKey(snkPath);
            return options;
        }


        /// <summary>
        /// Processes the specified module.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected abstract bool Process(AssemblyStitcherContext context);
    }
}
