namespace StitcherBoy.Weaving
{
    using System.Linq;
    using dnlib.DotNet;
    using Project;

    internal class Stitchers
    {
        public StitchersLoader StitcherLoader { get; set; }

        public bool Process(string assemblyPath, string projectPath, string solutionPath)
        {
            var project = new ProjectDefinition(projectPath);
            assemblyPath = assemblyPath ?? project.TargetPath;
            var module = ModuleDefMD.Load(assemblyPath);
            var clients = StitcherLoader.GetRetouchers(project, projectPath, solutionPath).ToArray();
            return false;
        }
    }
}
