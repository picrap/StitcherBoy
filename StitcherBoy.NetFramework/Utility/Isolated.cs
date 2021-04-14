namespace StitcherBoy.Utility
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Runs a method on an isolated instance
    /// </summary>
    public static class Isolated
    {
        /// <summary>
        /// Runs the specified method in a separated <see cref="AppDomain" />.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="run">The run.</param>
        /// <param name="assemblyNames">The assembly names.</param>
        /// <returns></returns>
        public static TResult Run<TInstance, TResult>(Func<TInstance, TResult> run, params string[] assemblyNames)
            where TInstance : MarshalByRefObject
        {
            AppDomain appDomain = null;
            try
            {
                var setup = new AppDomainSetup();
                appDomain = AppDomain.CreateDomain("StitcherBoy", null, setup);
                {
                    foreach (var assemblyName in assemblyNames)
                        appDomain.Load(new AssemblyName(assemblyName));
                    var instance = (TInstance)appDomain.CreateInstanceAndUnwrap(typeof(TInstance).Assembly.FullName, typeof(TInstance).FullName);
                    return run(instance);
                }
            }
            finally
            {
                if (appDomain is not null)
                    AppDomain.Unload(appDomain);
            }
        }
    }
}
