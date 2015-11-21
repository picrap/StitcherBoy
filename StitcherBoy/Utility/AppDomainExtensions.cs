namespace StitcherBoy.Utility
{
    using System;
    using System.IO;

    /// <summary>
    /// Extensions to AppDomain
    /// </summary>
    internal static class AppDomainExtensions
    {
        public static TInstance CreateInstanceAndUnwrap<TInstance>(this AppDomain appDomain)
            where TInstance : MarshalByRefObject
        {
            var instanceType = typeof(TInstance);
            var assembly = instanceType.Assembly;
            var assemblyName = assembly.GetName().ToString();
            appDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs e)
            {
                if (e.Name == assemblyName)
                    return assembly;
                return null;
            };
            var cwd = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(assembly.Location));
                return (TInstance)appDomain.CreateInstanceAndUnwrap(assemblyName, instanceType.FullName);
            }
            finally
            {
                Directory.SetCurrentDirectory(cwd);
            }
        }
    }
}
