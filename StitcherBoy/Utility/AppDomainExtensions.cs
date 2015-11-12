namespace StitcherBoy.Utility
{
    using System;

    /// <summary>
    /// Extensions to AppDomain
    /// </summary>
    internal static class AppDomainExtensions
    {
        public static TInstance CreateInstanceAndUnwrap<TInstance>(this AppDomain appDomain)
            where TInstance : MarshalByRefObject
        {
            var instanceType = typeof(TInstance);
            return (TInstance)appDomain.CreateInstanceAndUnwrap(instanceType.Assembly.GetName().ToString(), instanceType.FullName);
        }
    }
}
