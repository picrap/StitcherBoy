namespace StitcherBoy
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using SticherBoy;

    public class Stitcher: MarshalByRefObject
    {
        public void Run(string projectPath)
        {
            var project = new ProjectDefinition(projectPath);
            var rawAssembly = File.ReadAllBytes(project.TargetPath);
            var assembly = Assembly.ReflectionOnlyLoad(rawAssembly);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly.GetName(), AssemblyBuilderAccess.RunAndSave);
        }
    }
}