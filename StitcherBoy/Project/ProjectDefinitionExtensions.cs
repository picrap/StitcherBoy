// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Project
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utility;

    /// <summary>
    /// Extensions to <see cref="ProjectDefinition"/>
    /// </summary>
    public static class ProjectDefinitionExtensions
    {
        /// <summary>
        /// Gets the references (recursive).
        /// </summary>
        /// <param name="projectDefinition">The project.</param>
        /// <param name="assemblyReferenceSelector">The assembly reference selector.</param>
        /// <returns></returns>
        public static IEnumerable<AssemblyReference> GetReferences(this ProjectDefinition projectDefinition, Func<AssemblyReference, bool> assemblyReferenceSelector = null)
        {
            var loadedAssemblies = new Dictionary<string, AssemblyReference>();
            var fetchAssemblies = new Queue<AssemblyReference>();
            Select(projectDefinition.References, assemblyReferenceSelector).ForAll(r => fetchAssemblies.Enqueue(r));
            // gets remaining assemblies
            while (fetchAssemblies.Count > 0)
            {
                var fetchAssembly = fetchAssemblies.Dequeue();
                if (loadedAssemblies.ContainsKey(fetchAssembly.AssemblyName.FullName))
                    continue;

                Select(fetchAssembly.References, assemblyReferenceSelector).ForAll(r => fetchAssemblies.Enqueue(r));
                loadedAssemblies[fetchAssembly.AssemblyName.FullName] = fetchAssembly;
            }
            return loadedAssemblies.Values;
        }

        private static IEnumerable<AssemblyReference> Select(this IEnumerable<AssemblyReference> references, Func<AssemblyReference, bool> assemblyReferenceSelector)
        {
            if (assemblyReferenceSelector == null)
                return references;
            return references.Where(assemblyReferenceSelector);
        }
    }
}

