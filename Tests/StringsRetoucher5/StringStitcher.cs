namespace StringsRetoucher
{
    using StitcherBoy.Weaving.Build;

    public class StringStitcher : AssemblyStitcher
    {
        protected override bool Process(AssemblyStitcherContext context)
        {
            foreach (var type in context.Module.Types)
                foreach (var methodDef in type.Methods)
                    foreach (var instruction in methodDef.Body.Instructions)
                        if (instruction.Operand is string s)
                            instruction.Operand = s + " I was here";

            return true;
        }
    }
}