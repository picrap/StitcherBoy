
namespace StringsRetoucher
{
    using System.IO;
    using StitcherBoy.Reflection;

    public class RetoucherApplication : StitcherTask<StringStitcher>
    {
        public static void Main(string[] args)
        {
            //Run(new RetoucherApplication(), args);
            //var assemblyPath = Path.GetFullPath(@"..\..\..\TestApplication\obj\Debug\TestApplication.exe ");
            var assemblyPath = Path.GetFullPath(@"..\..\..\TestApplication5\obj\Debug\net5.0\TestApplication5.dll");
            using (var module = new ModuleManager(assemblyPath, true, true))
            {
                module.Write(null);
            }
        }
    }
}
