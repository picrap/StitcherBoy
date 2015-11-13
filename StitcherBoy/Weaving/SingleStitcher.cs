namespace StitcherBoy.Weaving
{
    using System;
    using dnlib.DotNet;
    using Project;

    public abstract class SingleStitcher : MarshalByRefObject
    {
        public bool Process(string assemblyPath, string projectPath, string solutionPath)
        {
            var project = new ProjectDefinition(projectPath);
            assemblyPath = assemblyPath ?? project.TargetPath;
            var module = ModuleDefMD.Load(assemblyPath);
            var ok = Process(module);
            return ok;
        }

        protected abstract bool Process(ModuleDefMD moduleDef);
    }
}
