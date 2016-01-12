using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationServiceSettings : ILocationServiceSettings
    {
        public void Show()
        {
           Windows.System.Launcher.LaunchUriAsync(new System.Uri("ms-settings-location:"));
        }
    }
}
