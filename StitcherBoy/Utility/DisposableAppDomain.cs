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
        /// Initializes a new instance of the <see cref="DisposableAppDomain"/> class.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        public DisposableAppDomain(string friendlyName)
        {
            AppDomain = AppDomain.CreateDomain(friendlyName);
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
