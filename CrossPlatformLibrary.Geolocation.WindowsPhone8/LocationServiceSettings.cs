using System;

using Windows.System;

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationServiceSettings : ILocationServiceSettings
    {
        public void Show()
        {
            Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
        }
    }
}