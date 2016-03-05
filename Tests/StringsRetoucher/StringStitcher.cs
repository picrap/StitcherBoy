namespace StringsRetoucher
{
    using System;
    using dnlib.DotNet;
    using StitcherBoy.Project;
    using StitcherBoy.Weaving;

    public class StringStitcher: SingleStitcher
    {
        protected override bool Process(StitcherContext context)
        {
            var r = context.Project.References;

            var a = AppDomain.CurrentDomain;
            var n = a.FriendlyName;
            return false;
        }
    }
}