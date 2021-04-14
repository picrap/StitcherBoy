
using StringsRetoucher;

RetoucherApplication.Run(args);

namespace StringsRetoucher
{
    public class RetoucherApplication : StitcherTask<StringStitcher>
    {
        public static void Run(string[] args)
        {
            Run(new RetoucherApplication(), args, false);
        }
    }
}
