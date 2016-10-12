#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving.Build
{
    using System.Collections.Generic;
    using dnlib.DotNet;
 public   class AssemblyStitcherResolver: AssemblyResolver
    {
        protected override IEnumerable<string> GetModuleSearchPaths(ModuleDef module)
        {
            return base.GetModuleSearchPaths(module);
        }

        protected override IEnumerable<string> FindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            return base.FindAssemblies(assembly, sourceModule, matchExactly);
        }

        protected override IEnumerable<string> PostFindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
        {
            return base.PostFindAssemblies(assembly, sourceModule, matchExactly);
        }
    }
}
