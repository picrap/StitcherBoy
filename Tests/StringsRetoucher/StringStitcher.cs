namespace StringsRetoucher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using dnlib.DotNet;
    using StitcherBoy.Weaving;
    using StitcherBoy.Weaving.MSBuild;
    using StitcherBoy.Weaving.MSBuild.Project;

    public class StringStitcher : SingleStitcher
    {
        public string Configuration { get; set; }

        private void F(AssemblyReference[] r)
        {
            Console.WriteLine(r);
        }

        protected override bool Process(StitcherContext context)
        {
            //foreach (var type in context.Module.Types)
            //{
            //    foreach (var methodDef in type.Methods)
            //    {
            //        foreach (var instruction in methodDef.Body.Instructions)
            //        {
            //            var s = instruction.Operand as string;
            //            if (s != null)
            //            {
            //                s = s + " I was here";
            //                instruction.Operand = s;
            //            }
            //        }
            //    }
            //}

            //return true;

            var r = context.Project.References.ToArray();
            F(r);
            var a = AppDomain.CurrentDomain;
            var n = a.FriendlyName;
            return false;
        }
    }
}