namespace StringsRetoucher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StitcherBoy.Project;
    using StitcherBoy.Weaving;

    public class StringStitcher : SingleStitcher
    {
        public string Configuration { get; set; }

        private void F(AssemblyReference[] r)
        {
            Console.WriteLine(r);
        }

        protected override bool Process(StitcherContext context)
        {
            var r = context.Project.References.ToArray();
            F(r);
            var a = AppDomain.CurrentDomain;
            var n = a.FriendlyName;
            return false;
        }
    }
}