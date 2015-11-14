namespace StitcherBoy.Weaving
{
    using System;
    using System.IO;
    using dnlib.DotNet;
    using dnlib.DotNet.Writer;
    using Project;

    public abstract class SingleStitcher : MarshalByRefObject
    {
        public bool Process(string assemblyPath, string projectPath, string solutionPath)
        {
            var project = new ProjectDefinition(projectPath);
            assemblyPath = assemblyPath ?? project.TargetPath;
            var tempAssemblyPath = assemblyPath + ".out";
            bool ok;
            using (var module = ModuleDefMD.Load(assemblyPath))
            {
                module.LoadPdb();
                ok = Process(module);
                if (ok)
                {
                    if (module.IsILOnly)
                        module.Write(tempAssemblyPath, SetWriterOptions(project, module, new ModuleWriterOptions(module)));
                    else
                        module.NativeWrite(tempAssemblyPath, SetWriterOptions(project, module, new NativeModuleWriterOptions(module)));
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
            return ok;
        }

        private TOptions SetWriterOptions<TOptions>(ProjectDefinition project, ModuleDefMD moduleDef, TOptions options)
            where TOptions : ModuleWriterOptionsBase
        {
            options.WritePdb = true;
            options.StrongNameKey = GetSNK(project);
            return options;
        }

        private StrongNameKey GetSNK(ProjectDefinition project)
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

        protected abstract bool Process(ModuleDefMD moduleDef);
    }
}
