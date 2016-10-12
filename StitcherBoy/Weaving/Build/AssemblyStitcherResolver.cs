#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;

    /// <summary>
    /// Assembly resolver, with extra dependencies
    /// </summary>
    /// <seealso cref="dnlib.DotNet.AssemblyResolver" />
    public class AssemblyStitcherResolver : AssemblyResolver
    {
        private readonly string[] _extraAssemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyStitcherResolver"/> class.
        /// </summary>
        /// <param name="extraAssemblies">The extra assemblies.</param>
        public AssemblyStitcherResolver(IEnumerable<string> extraAssemblies)
        {
            _extraAssemblies = extraAssemblies.ToArray();
        }

        /// <summary>
        /// Called after <see cref="M:dnlib.DotNet.AssemblyResolver.PreFindAssemblies(dnlib.DotNet.IAssembly,dnlib.DotNet.ModuleDef,System.Boolean)" /> (if it fails)
        /// </summary>
        /// <param name="assembly">Assembly to find</param>
        /// <param name="sourceModule">The module that needs to resolve an assembly or <c>null</c></param>
        /// <param name="matchExactly">We're trying to find an exact match</param>
        /// <returns>
        ///   <c>null</c> or an enumerable of full paths to try
        /// </returns>
        protected override IEnumerable<string> FindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            return base.FindAssemblies(assembly, sourceModule, matchExactly).Concat(FindExtraAssemblies(assembly));
        }

        /// <summary>
        /// Finds the extra assemblies, if any matches the requested.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        private IEnumerable<string> FindExtraAssemblies(IAssembly assembly)
        {
            var fileName = new AssemblyName(assembly.FullNameToken).Name;
            foreach (var extraAssembly in _extraAssemblies)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(extraAssembly);
                if (string.Equals(fileName, assemblyName, StringComparison.InvariantCultureIgnoreCase))
                    yield return extraAssembly;
            }
        }
    }
}
