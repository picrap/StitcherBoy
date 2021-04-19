namespace StringsRetoucher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StitcherBoy.Weaving.Build;

    public class StringStitcher : AssemblyStitcher
    {
        protected override bool Process(AssemblyStitcherContext context)
        {
#if DEBUG1
            var r = new Resolver(context.AssemblyResolver);
            var typeRef = context.Module.EntryPoint.Parameters[0].Type.Next.TryGetTypeRef();
            var fullType = r.Resolve(typeRef, context.Module);
#endif
            foreach (var type in context.Module.Types)
                foreach (var methodDef in type.Methods)
                    foreach (var instruction in methodDef.Body.Instructions)
                        if (instruction.Operand is string s)
                            instruction.Operand = s + " I was here";

            return true;
        }
    }
}