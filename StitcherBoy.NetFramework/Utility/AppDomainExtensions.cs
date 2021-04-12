// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
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
            var cwd = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(assembly.Location));
                return (TInstance)appDomain.CreateInstanceAndUnwrap(assembly.GetName().ToString(), instanceType.FullName);
            }
            finally
            {
                Directory.SetCurrentDirectory(cwd);
            }
        }
    }
}
