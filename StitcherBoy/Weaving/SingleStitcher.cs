// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Weaving
{
    using System;
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
        /// <param name="buildID"></param>
        /// <param name="buildTime"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Could not find assembly to stitch</exception>
        public bool Process(string assemblyPath, string projectPath, string solutionPath, Guid buildID, DateTime buildTime)
        {
            var project = new ProjectDefinition(projectPath);
            assemblyPath = ExistingPath(assemblyPath) ?? ExistingPath(project.IntermediatePath) ?? ExistingPath(project.TargetPath);
            if (assemblyPath == null)
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
                    };
                    ok = Process(context);
                }
                catch (Exception e)
                {
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
                File.Copy(tempAssemblyPath, assemblyPath, true);
                File.Delete(tempAssemblyPath);
            }
            return success;
        }

        /// <summary>
        /// Returns the path if the file exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string ExistingPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            if (!File.Exists(path))
                return null;
            return path;
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
