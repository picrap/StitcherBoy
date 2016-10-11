#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving
{
    using System;
    using System.Collections.Specialized;
    using System.Security.Cryptography.X509Certificates;
    using Logging;

    internal interface IStitcher
    {
        ILogging Logging { get; set; }

        bool Process(StringDictionary parameters, Guid buildID, DateTime buildTime);
    }
}
