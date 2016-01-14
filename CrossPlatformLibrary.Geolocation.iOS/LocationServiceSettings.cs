using CrossPlatformLibrary.Geolocation.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrossPlatformLibrary.Geolocation.Exceptions;
using System.Threading.Tasks;

#if __UNIFIED__
using CoreLocation;
using Foundation;
using UIKit;
#else
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationServiceSettings : ILocationServiceSettings
    {
        private readonly CLLocationManager manager;

        public LocationServiceSettings()
        {
            this.manager = LocationService.GetLocationManager();
        }

        public void Show()
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
        }
    }
}
