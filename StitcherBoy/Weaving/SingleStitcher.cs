// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Weaving
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using dnlib.DotNet;
    using dnlib.DotNet.Pdb;
    using dnlib.DotNet.Writer;
    using Logging;
    using Microsoft.Build.Evaluation;
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
        /// Initializes a new instance of the <see cref="SingleStitcher"/> class.
        /// </summary>
        public SingleStitcher()
        {
            ProjectDefinition.LoadError += OnProjectDefinitionLoadError;
        }

        /// <summary>
        /// Processes the specified assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="projectPath">The project path.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="buildID">The build identifier.</param>
        /// <param name="buildTime">The build time.</param>
        /// <param name="entryAssemblyPath">The entry assembly path.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Could not find assembly to stitch</exception>
        /// <exception cref="System.InvalidOperationException">Could not find assembly to stitch</exception>
        public bool Process(string assemblyPath, string projectPath, string solutionPath, string configuration, string platform, Guid buildID, DateTime buildTime, string entryAssemblyPath)
        {
            var globalProperties = new Dictionary<string, string>
            {
                { "Configuration", (configuration ?? "Release").Trim() },
                { "Platform", (platform ?? "AnyCPU").Trim() }
            };
            var project = new ProjectDefinition(projectPath, Path.GetDirectoryName(assemblyPath), null, CreateAssemblyResolver(), new ProjectCollection(globalProperties));
            assemblyPath = assemblyPath ?? project.TargetPath;
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
                            Platform = platform,
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
                            module.Write(assemblyPath, SetWriterOptions(project, module, moduleWriterOptions));
                        }
                        else
                        {
                            var nativeModuleWriterOptions = new NativeModuleWriterOptions(module);
                            nativeModuleWriterOptions.WritePdb = true;
                            nativeModuleWriterOptions.PdbFileName = pdbPath;
                            module.NativeWrite(assemblyPath, SetWriterOptions(project, module, nativeModuleWriterOptions));
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
        /// Called when [project definition load error].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ProjectDefinitionLoadErrorEventArgs"/> instance containing the event data.</param>
        protected virtual void OnProjectDefinitionLoadError(object sender, ProjectDefinitionLoadErrorEventArgs e)
        {
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

        /// <summary>
        /// Creates the assembly resolver.
        /// </summary>
        /// <returns></returns>
        protected virtual IAssemblyResolver CreateAssemblyResolver() => new AssemblyResolver();
    }
}
