using System;

#if __UNIFIED__
using CoreLocation;
using Foundation;
using UIKit;
#else
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace CrossPlatformLibrary.Geolocation.Extensions
{
    internal static class CLLocationManagerExtensions
    {
        internal static void RequestAuthorization(this CLLocationManager manager)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var info = NSBundle.MainBundle.InfoDictionary;

                if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                {
                    manager.RequestWhenInUseAuthorization(); // This call causes an AuthorizationChanged event
                }
                else if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
                {
                    manager.RequestAlwaysAuthorization(); // This call causes an AuthorizationChanged event
                }
                else
                {
                    throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");
                }
            }
        }
    }
}
