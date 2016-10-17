#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving.Build
{
    using System;
    using System.Diagnostics;
    using dnlib.DotNet;

    /// <summary>
    /// Represents a dependency
    /// </summary>
    [DebuggerDisplay("{Path}, private={IsPrivate}")]
    public class AssemblyDependency
    {
        /// <summary>
        /// Gets a value indicating whether this instance is private.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrivate { get; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; }

        private ModuleDef _module;

        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        [Obsolete("Use only if you are aware of all possible failures. Otherwise use ModuleManager.")]
        public ModuleDef Module
        {
            get
            {
                if (_module == null)
                    _module = ModuleDefMD.Load(Path);
                return _module;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDependency"/> class.
        /// </summary>
        /// <param name="isPrivate">if set to <c>true</c> [is private].</param>
        /// <param name="path">The path.</param>
        public AssemblyDependency(string path, bool isPrivate)
        {
            IsPrivate = isPrivate;
            Path = path;
        }
    }
}
