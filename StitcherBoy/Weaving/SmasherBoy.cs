namespace StitcherBoy.Weaving
{
    using System;

    /// <summary>
    /// Smash the mirror, Tommy.
    /// </summary>
    internal class SmasherBoy : MarshalByRefObject
    {
        /// <summary>
        /// Processes the specified assembly/project.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="projectPath">The project path.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns></returns>
        public bool Process(string assemblyPath, string projectPath, string solutionPath)
        {
            var processor = new Stitchers();
            var clientLoader = new StitchersLoader();
            processor.StitcherLoader = clientLoader;
            return processor.Process(assemblyPath, projectPath, solutionPath);
        }
    }
}
