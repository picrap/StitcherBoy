// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Weaving
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using dnlib.DotNet;
    using dnlib.DotNet.Writer;
    using Logging;
    using Project;

    /// <summary>
    /// Single stitcher base class
    /// </summary>
    public abstract class SingleStitcher
    {
        /// <summary>
        /// Gets or sets the logging.
        /// </summary>
        /// <value>
        /// The logging.
        /// </value>
        public ILogging Logging { get; set; }

        /// <summary>
        /// Processes the specified assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="projectPath">The project path.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="configuration"></param>
        /// <param name="buildID">The build identifier.</param>
        /// <param name="buildTime">The build time.</param>
        /// <param name="entryAssemblyPath">The entry assembly path.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Could not find assembly to stitch</exception>
        /// <exception cref="InvalidOperationException">Could not find assembly to stitch</exception>
        public bool Process(string assemblyPath, string projectPath, string solutionPath, string configuration, Guid buildID, DateTime buildTime, string entryAssemblyPath)
        {
            var globalProperties = new Dictionary<string, string> { { "Configuration", configuration ?? "Release" } };
            var project = new ProjectDefinition(projectPath, Path.GetDirectoryName(assemblyPath), globalProperties);
            assemblyPath = assemblyPath ?? project.TargetPath;
            if (assemblyPath == null || !File.Exists(assemblyPath))
                throw new InvalidOperationException("Could not find assembly to stitch");
            var tempAssemblyPath = Path.Combine(Path.GetDirectoryName(assemblyPath), Path.GetFileNameWithoutExtension(assemblyPath) + ".2" + Path.GetExtension(assemblyPath));
            bool ok;
            bool success = true;
            using (var module = ModuleDefMD.Load(assemblyPath))
            {
                module.LoadPdb();
                try
                {
                    var context = new StitcherContext
                    {
                        Module = module,
                        AssemblyPath = assemblyPath,
                        BuildTime = buildTime,
                        BuildID = buildID,
                        Project = project,
                        ProjectPath = projectPath,
                        SolutionPath = solutionPath,
                        TaskAssemblyPath = entryAssemblyPath,
                        Configuration = configuration,
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
                        module.Write(tempAssemblyPath, SetWriterOptions(project, module, moduleWriterOptions));
                    }
                    else
                    {
                        var nativeModuleWriterOptions = new NativeModuleWriterOptions(module);
                        nativeModuleWriterOptions.WritePdb = true;
                        module.NativeWrite(tempAssemblyPath, SetWriterOptions(project, module, nativeModuleWriterOptions));
                    }
                }
            }
            // here the module is released
            if (ok)
            {
                // this is just in case there was a hard link on the target file
                // (not sure it's not destroyed by build anyway)
                Replace(tempAssemblyPath, assemblyPath);
                // also, the pdb has to be overwritten
                var pdbExtension = ".pdb";
                var tempPdbPath = ChangeExtension(tempAssemblyPath, pdbExtension);
                var pdbPath = ChangeExtension(assemblyPath, pdbExtension);
                Replace(tempPdbPath, pdbPath);
            }
            return success;
        }

        private static void Replace(string sourcePath, string targetPath)
        {
            File.Copy(sourcePath, targetPath, true);
            File.Delete(sourcePath);
        }

        /// <summary>
        /// Changes the extension, given a full path, returns a related path with different extension
        /// Right, .NET path manipulation functions are POOR
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="newExtension">The new extension.</param>
        /// <returns></returns>
        private string ChangeExtension(string path, string newExtension)
        {
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(directory, fileName + newExtension);
        }

        private static TOptions SetWriterOptions<TOptions>(ProjectDefinition project, ModuleDefMD moduleDef, TOptions options)
            where TOptions : ModuleWriterOptionsBase
        {
            options.WritePdb = true;
            options.StrongNameKey = GetSNK(project);
            return options;
        }

        private static StrongNameKey GetSNK(ProjectDefinition project)
        {
            var signAssembly = project.GetBoolProperty("SignAssembly") ?? false;
            var keyFile = project.GetProperty("AssemblyOriginatorKeyFile");
            if (signAssembly && keyFile != null)
            {
                if (File.Exists(keyFile))
                {
                    var snk = new StrongNameKey(keyFile);
                    return snk;
                }
            }
            return null;
        }

        /// <summary>
        /// Processes the specified module.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected abstract bool Process(StitcherContext context);
    }
}
