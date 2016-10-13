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
    using dnlib.DotNet.Pdb;
    using dnlib.DotNet.Writer;
    using Logging;

    /// <summary>
    /// Assembly stitcher
    /// </summary>
    /// <seealso cref="StitcherBoy.Weaving.IStitcher" />
    public abstract class AssemblyStitcher : IStitcher
    {
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
            var assemblyPath = parameters["AssemblyPath"];
            var literalSignAssembly = parameters["SignAssembly"];
            var referencePath = parameters["ReferencePath"];
            var referenceCopyLocalPaths = parameters["ReferenceCopyLocalPaths"];
            bool signAssembly = false;
            if (literalSignAssembly != null)
                bool.TryParse(literalSignAssembly, out signAssembly);
            var assemblyOriginatorKeyFile = signAssembly ? parameters["AssemblyOriginatorKeyFile"] : null;
            if (assemblyPath == null || !File.Exists(assemblyPath))
                throw new InvalidOperationException("Could not find assembly to stitch");
            var pdbExtension = ".pdb";
            var pdbPath = ChangeExtension(assemblyPath, pdbExtension);
            bool success = true;
            var tempAssemblyPath = assemblyPath + ".2";
            File.Copy(assemblyPath, tempAssemblyPath, true);
            try
            {
                using (var module = ModuleDefMD.Load(tempAssemblyPath))
                {
                    if (File.Exists(pdbPath))
                        module.LoadPdb(PdbImplType.MicrosoftCOM, File.ReadAllBytes(pdbPath));
                    bool ok;
                    try
                    {
                        var context = new AssemblyStitcherContext
                        {
                            Module = module,
                            Dependencies = EnumerateDependencies(referencePath, referenceCopyLocalPaths).ToArray(),
                            AssemblyPath = assemblyPath,
                            BuildID = buildID,
                            BuildTime = buildTime,
                            TaskAssemblyPath = entryAssemblyPath,
                        };
                        context.AssemblyResolver = new AssemblyStitcherResolver(context.Dependencies.Select(d => d.Path));
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
                        if (module.IsILOnly)
                        {
                            var moduleWriterOptions = new ModuleWriterOptions(module);
                            moduleWriterOptions.WritePdb = true;
                            moduleWriterOptions.PdbFileName = pdbPath;
                            module.Write(assemblyPath, SetWriterOptions(assemblyOriginatorKeyFile, moduleWriterOptions));
                        }
                        else
                        {
                            var nativeModuleWriterOptions = new NativeModuleWriterOptions(module);
                            nativeModuleWriterOptions.WritePdb = true;
                            nativeModuleWriterOptions.PdbFileName = pdbPath;
                            module.NativeWrite(assemblyPath, SetWriterOptions(assemblyOriginatorKeyFile, nativeModuleWriterOptions));
                        }
                    }
                }
            }
            finally
            {
                File.Delete(tempAssemblyPath);
            }
            return success;
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

        /// <summary>
        /// Changes the extension, given a full path, returns a related path with different extension
        /// Right, .NET path manipulation functions are POOR
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="newExtension">The new extension.</param>
        /// <returns></returns>
        private static string ChangeExtension(string path, string newExtension)
        {
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(directory, fileName + newExtension);
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
