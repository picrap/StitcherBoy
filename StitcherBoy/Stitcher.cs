namespace StitcherBoy
{
    using System;
    using System.Reflection;
    using Logging;
    using Utility;
    using Weaving;

    /// <summary>
    ///
    /// </summary>
    public static class Stitcher
    {
        /// <summary>
        /// Runs the specified arguments.
        /// </summary>
        /// <typeparam name="TStitcher">The type of the stitcher.</typeparam>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static int Run<TStitcher>(string[] args)
            where TStitcher : IStitcher, new()
        {
            var logging = new ConsoleLogging();
            try
            {
                var stitcher = new TStitcher { Logging = logging };
                var arguments = args.ParseArguments();
                stitcher.Inject(arguments);
                return stitcher.Process(arguments, Assembly.GetEntryAssembly().Location) ? 0 : 1;
            }
            catch (Exception e)
            {
                logging.Write("Unhandled exception: {0}", e);
            }

            return 2;
        }
    }
}