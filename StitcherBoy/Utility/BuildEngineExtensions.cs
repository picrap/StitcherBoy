// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

namespace StitcherBoy.Utility
{
    using System;
    using Microsoft.Build.Framework;

    /// <summary>
    /// Entensions to <see cref="IBuildEngine4"/>
    /// </summary>
    public static class BuildEngineExtensions
    {
        /// <summary>
        /// Gets the registered task object.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="buildEngine">The build engine.</param>
        /// <param name="key">The key.</param>
        /// <param name="ctor">The ctor.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <returns></returns>
        public static TValue GetRegisteredTaskObject<TValue>(this IBuildEngine4 buildEngine, string key, Func<TValue> ctor, RegisteredTaskObjectLifetime lifetime = RegisteredTaskObjectLifetime.Build)
        {
            // when run as app, we don't have a filled Task
            if (buildEngine == null)
                return ctor();

            // otherwise, this works
            var o = buildEngine.GetRegisteredTaskObject(key, lifetime);
            if (o == null)
            {
                o = ctor();
                buildEngine.RegisterTaskObject(key, o, lifetime, false);
            }
            return (TValue) o;
        }
    }
}
