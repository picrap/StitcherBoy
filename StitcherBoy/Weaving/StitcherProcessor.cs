// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Weaving
{
    using System;
    using System.Linq;
    using Logging;

    /// <summary>
    /// Invokes the actual stitcher
    /// </summary>
    public class StitcherProcessor : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets the logging.
        /// </summary>
        /// <value>
        /// The logging.
        /// </value>
        public ILogging Logging { get; set; }

        private Type _type;

        /// <summary>
        /// Loads the specified type (given by name and assembly).
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public void Load(string typeName)
        {
            _type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName)).First(t => t != null);
        }

        /// <summary>
        /// Processes the specified assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="projectPath">The project path.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="buildID">The build identifier.</param>
        /// <param name="buildTime">The build time.</param>
        /// <param name="entryAssemblyPath">The entry assembly path.</param>
        /// <returns></returns>
        public bool Process(string assemblyPath, string projectPath, string solutionPath, Guid buildID, DateTime buildTime, string entryAssemblyPath)
        {
            var instance = (SingleStitcher)Activator.CreateInstance(_type);
            instance.Logging = Logging;
            return instance.Process(assemblyPath, projectPath, solutionPath, buildID, buildTime, entryAssemblyPath);
        }
    }
}
