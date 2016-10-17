#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving.Build
{
    using System;
    using dnlib.DotNet;

    /// <summary>
    /// Context for <see cref="AssemblyStitcher"/>
    /// </summary>
    public class AssemblyStitcherContext
    {
        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public IAssemblyResolver AssemblyResolver { get; set; }

        /// <summary>
        /// Gets or sets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public ModuleDefMD Module { get; set; }

        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        public AssemblyDependency[] Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the assembly path.
        /// </summary>
        /// <value>
        /// The assembly path.
        /// </value>
        public string AssemblyPath { get; set; }

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

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public string Configuration { get; set; }
    }
}
