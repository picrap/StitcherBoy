// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

namespace StitcherBoy.Weaving.Build
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using Reflection;

    /// <summary>
    /// An assembly resolver
    /// </summary>
    /// <seealso cref="dnlib.DotNet.IAssemblyResolver" />
    public partial class ReadonlyAssemblyResolver : IAssemblyResolver
    {
        private readonly IDictionary<IAssembly, AssemblyDef> _cache =
            new ConcurrentDictionary<IAssembly, AssemblyDef>(AssemblyComparer);

        private readonly string[] _extraAssemblies;
        private static readonly AssemblyEqualityComparer AssemblyComparer = new AssemblyEqualityComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadonlyAssemblyResolver"/> class.
        /// </summary>
        /// <param name="extraAssemblies">The extra assemblies.</param>
        public ReadonlyAssemblyResolver(IEnumerable<string> extraAssemblies)
        {
            _extraAssemblies = extraAssemblies.ToArray();
        }

        /// <summary>
        /// Resolves the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="sourceModule">The source module.</param>
        /// <returns></returns>
        public virtual AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
        {
            return ResolveFromCache(assembly) ?? ResolveFromReference(assembly)
                //?? ResolveFromGAC(assembly, sourceModule)
                ;
        }

        /// <summary>
        /// Resolves from cache.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        protected virtual AssemblyDef ResolveFromCache(IAssembly assembly)
        {
            AssemblyDef assemblyDef;
            _cache.TryGetValue(assembly, out assemblyDef);
            return assemblyDef;
        }

        /// <summary>
        /// Resolves from list.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="paths">The paths.</param>
        /// <returns></returns>
        protected virtual AssemblyDef ResolveFromList(IAssembly assembly, IEnumerable<string> paths)
        {
            foreach (var extraAssembly in paths)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(extraAssembly);
                if (string.Equals(assembly.Name.String, assemblyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var assemblyDef = LoadFile(extraAssembly);
                    // comparison here is strict. We'll try lose later
                    if (AssemblyComparer.Equals(assembly, assemblyDef))
                        return assemblyDef;
                }
            }
            return null;
        }

        /// <summary>
        /// Resolves from given reference.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        protected virtual AssemblyDef ResolveFromReference(IAssembly assembly)
        {
            return ResolveFromList(assembly, _extraAssemblies);
        }

        /// <summary>
        /// Resolves from gac.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        protected virtual AssemblyDef ResolveFromGAC(IAssembly assembly, ModuleDef source)
        {
            return ResolveFromList(assembly, FindAssembliesGacExactly(assembly, source));
        }

        private AssemblyDef LoadFile(string path)
        {
            var assemblyBytes = File.ReadAllBytes(path);
            var assemblyDef = AssemblyDef.Load(assemblyBytes);
            assemblyDef.ManifestModule.Location = path;
            _cache[assemblyDef] = assemblyDef;
            return assemblyDef;
        }

        /// <summary>
        /// Add an assembly to the assembly cache
        /// </summary>
        /// <param name="asm">The assembly</param>
        /// <returns>
        /// <c>true</c> if <paramref name="asm" /> is cached, <c>false</c> if it's not
        /// cached because some other assembly with the exact same full name has already been
        /// cached or if <paramref name="asm" /> is <c>null</c>.
        /// </returns>
        public bool AddToCache(AssemblyDef asm)
        {
            if (_cache.ContainsKey(asm))
                return false;
            _cache[asm] = asm;
            return true;
        }

        /// <summary>
        /// Removes the assembly from the cache
        /// </summary>
        /// <param name="asm">The assembly</param>
        /// <returns>
        /// <c>true</c> if it was removed, <c>false</c> if it wasn't removed since it
        /// wasn't in the cache or if <paramref name="asm" /> was <c>null</c>
        /// </returns>
        public bool Remove(AssemblyDef asm)
        {
            return _cache.Remove(asm);
        }

        /// <summary>
        /// Clears the cache and calls <see cref="M:System.IDisposable.Dispose" /> on each cached module.
        /// Use <see cref="M:dnlib.DotNet.IAssemblyResolver.Remove(dnlib.DotNet.AssemblyDef)" /> to remove any assemblies you added yourself
        /// using <see cref="M:dnlib.DotNet.IAssemblyResolver.AddToCache(dnlib.DotNet.AssemblyDef)" /> before calling this method if you don't want
        /// them disposed.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Clear()
        {
            _cache.Clear();
        }
    }
}
