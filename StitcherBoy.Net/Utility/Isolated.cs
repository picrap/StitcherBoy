namespace StitcherBoy.Utility
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;

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
            AssemblyLoadContext context = null;
            try
            {
                context = new AssemblyLoadContext("StitcherBoy", true);
                var assembly = context.LoadFromAssemblyName(typeof(TInstance).Assembly.GetName());
                foreach (var assemblyName in assemblyNames)
                    context.LoadFromAssemblyName(new AssemblyName(assemblyName));
                var instance = (TInstance)assembly.CreateInstance(typeof(TInstance).FullName, false);
                return run(instance);
            }
            finally
            {
                context?.Unload();
            }
        }
    }
}
