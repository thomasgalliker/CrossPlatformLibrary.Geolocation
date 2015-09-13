using System;

using CrossPlatformLibrary.IoC;
using CrossPlatformLibrary.Tools.PlatformSpecific;

namespace CrossPlatformLibrary.Geolocation
{
    public class ContainerExtension : IContainerExtension
    {
        public void Initialize(ISimpleIoc container)
        {
            container.RegisterWithConvention<ILocationService>(new GeolocatorRegistrationConvention());
        }

        private class GeolocatorRegistrationConvention : DefaultRegistrationConvention
        {
            public override string InterfaceToClassNamingConvention(Type interfaceType)
            {
                // TODO GATH: Better idea: Rename Geolocator classes to LocationService in order to work with DefaultRegistrationConvention.
                return string.Format("{0}.{1}", interfaceType.Namespace, "Geolocator");
            }
        }
    }
}
