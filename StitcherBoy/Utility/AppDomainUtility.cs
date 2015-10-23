namespace StitcherBoy.Utility
{
    using System;

    internal static class AppDomainUtility
    {
        public static void InvokeSeparated<TMarshalByRef>(this Action<TMarshalByRef> action)
            where TMarshalByRef : MarshalByRefObject
        {
            var type = typeof(TMarshalByRef);
            var appDomain = AppDomain.CreateDomain(type.FullName + Guid.NewGuid());
            try
            {
                var remoteStitcher = (TMarshalByRef)appDomain.CreateInstanceAndUnwrap(type.Assembly.GetName().FullName, type.FullName);
                action(remoteStitcher);
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }

        }
    }
}