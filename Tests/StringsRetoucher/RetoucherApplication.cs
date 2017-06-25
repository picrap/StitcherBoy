
namespace StringsRetoucher
{
    using StitcherBoy.Reflection;

    public class RetoucherApplication: StitcherTask<StringStitcher>
    {
        public static void Main(string[] args)
        {
            //Run(new RetoucherApplication(), args);
            const string assemblyPath = @"obj\Debug\TestApplication.exe ";
            using (var module = new ModuleManager(assemblyPath, true, true))
            {
                module.Write(null);
            }
        }
    }
}
