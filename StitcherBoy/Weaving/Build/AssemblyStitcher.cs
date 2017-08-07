#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving.Build
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
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
        /// <param name="buildID">The build identifier.</param>
        /// <param name="buildTime">The build time.</param>
        /// <param name="entryAssemblyPath">The entry assembly path.</param>
        /// <returns></returns>
        public bool Process(StringDictionary parameters, Guid buildID, DateTime buildTime, string entryAssemblyPath)
        {
            var configuration = parameters["Configuration"];
            var assemblyPath = parameters["AssemblyPath"];
            var literalSignAssembly = parameters["SignAssembly"];
            var referencePath = parameters["ReferencePath"];
            var referenceCopyLocalPaths = parameters["ReferenceCopyLocalPaths"];
            bool signAssembly = false;
            if (literalSignAssembly != null)
                bool.TryParse(literalSignAssembly, out signAssembly);
            var assemblyOriginatorKeyFile = signAssembly ? parameters["AssemblyOriginatorKeyFile"] : null;

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
                        BuildID = buildID,
                        BuildTime = buildTime,
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
                    moduleHandler?.Write(assemblyOriginatorKeyFile);
            }

            var onModuleWritten = ModuleWritten;
            if (onModuleWritten != null)
                onModuleWritten(this, EventArgs.Empty);

            return success;
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
            if (paths != null)
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
