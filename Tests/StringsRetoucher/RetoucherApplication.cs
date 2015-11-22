
namespace StringsRetoucher
{
    public class RetoucherApplication: StitcherTask<StringStitcher>
    {
        public static void Main(string[] args) => Run(new RetoucherApplication(), args);
    }
}
