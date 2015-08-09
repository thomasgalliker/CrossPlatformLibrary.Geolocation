using CrossPlatformLibrary.Bootstrapping;
using CrossPlatformLibrary.IoC;

namespace CrossPlatformLibrary.Geolocation
{
    public class ContainerExtension : IContainerExtension
    {
        public void Initialize(ISimpleIoc container)
        {
            container.RegisterPlatformSpecific<ILocationService>();
        }
    }
}
