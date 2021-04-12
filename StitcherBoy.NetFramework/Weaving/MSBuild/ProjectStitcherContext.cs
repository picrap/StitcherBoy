// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

namespace StitcherBoy.Weaving.MSBuild
{
    using System;
    using dnlib.DotNet;
    using Project;

    /// <summary>
    /// Arguments to stitcher
    /// </summary>
    public class ProjectStitcherContext
    {
        /// <summary>
        /// Gets or sets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public ModuleDefMD Module { get; set; }
        /// <summary>
        /// Gets or sets the assembly path.
        /// </summary>
        /// <value>
        /// The assembly path.
        /// </value>
        public string AssemblyPath { get; set; }
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public ProjectDefinition Project { get; set; }
        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        /// <value>
        /// The project path.
        /// </value>
        public string ProjectPath { get; set; }
        /// <summary>
        /// Gets or sets the solution path.
        /// </summary>
        /// <value>
        /// The solution path.
        /// </value>
        public string SolutionPath { get; set; }
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public string Configuration { get; set; }
        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value>
        /// The platform.
        /// </value>
        public string Platform { get; set; }
        /// <summary>
        /// Gets or sets the build identifier.
        /// </summary>
        /// <value>
        /// The build identifier.
        /// </value>
        public Guid BuildID { get; set; }
        /// <summary>
        /// Gets or sets the build date.
        /// </summary>
        /// <value>
        /// The build date.
        /// </value>
        public DateTime BuildTime { get; set; }
        /// <summary>
        /// Gets or sets the entry task assembly path.
        /// </summary>
        /// <value>
        /// The entry assembly path.
        /// </value>
        public string TaskAssemblyPath { get; set; }
    }
}
