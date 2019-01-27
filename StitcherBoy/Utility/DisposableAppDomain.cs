// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Utility
{
    using System;

    internal class DisposableAppDomain : IDisposable
    {
        /// <summary>
        /// Gets the application domain.
        /// </summary>
        /// <value>
        /// The application domain.
        /// </value>
        public AppDomain AppDomain { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableAppDomain" /> class.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="basePath">The base path.</param>
        public DisposableAppDomain(string friendlyName, string basePath)
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = basePath;
            AppDomain = AppDomain.CreateDomain(friendlyName, null, setup);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            AppDomain.Unload(AppDomain);
        }
    }
}
