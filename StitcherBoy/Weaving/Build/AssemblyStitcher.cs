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
    using dnlib.DotNet;
    using dnlib.DotNet.Pdb;
    using dnlib.DotNet.Writer;
    using Logging;
    using Microsoft.Build.Evaluation;
    using MSBuild.Project;

    /// <summary>
    /// Assembly stitcher
    /// </summary>
    /// <seealso cref="StitcherBoy.Weaving.IStitcher" />
    public abstract class AssemblyStitcher : IStitcher
    {
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
            bool signAssembly;
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
                            AssemblyPath = assemblyPath,
                            BuildID = buildID,
                            BuildTime = buildTime,
                            TaskAssemblyPath = entryAssemblyPath,
                        };
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

        /// <summary>
        /// Creates the assembly resolver.
        /// </summary>
        /// <returns></returns>
        protected virtual IAssemblyResolver CreateAssemblyResolver() => new AssemblyResolver();
    }
}
