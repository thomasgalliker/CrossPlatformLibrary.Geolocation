# CrossPlatformLibrary.Geolocation

### Download and Install CrossPlatformLibrary.Geolocation
This library is available on NuGet: https://www.nuget.org/packages/CrossPlatformLibrary.Geolocation/
Use the following command to install CrossPlatformLibrary.Geolocation using NuGet package manager console:

    PM> Install-Package CrossPlatformLibrary.Geolocation

You can use this library in any .Net project which is compatible to PCL (e.g. Xamarin Android, iOS, Windows Phone, Windows Store, Universal Apps, etc.)

### Platform requirements
- Xamarin Android: Add `ACCESS_COARSE_LOCATION` and `ACCESS_FINE_LOCATION` permissions.
- Xamarin iOS: Set an NSLocationWhenInUseUsageDescription in Info.plist.
- Windows Phone 8: Add `ID_CAP_LOCATION` permission.

### API Usage
CrossPlatformLibrary.Geolocation provides a simple and platform-agnostic interface, ```ILocationService```.
Following example demonstrates how to get a GPS location:

```csharp
using CrossPlatformLibrary.Geolocation;
// ...

// Use dependency injection to retrieve platform-specific implementation for ILocationService
ILocationService locationService = ServiceLocator.Current.Resolve<ILocationService>(); 
Position position = await locationService.GetPositionAsync (timeout: 10000);

Console.WriteLine ("GPS latitude: {0}", position.Latitude);
Console.WriteLine ("GPS longitude: {0}", position.Longitude);
```

### License
This library is Copyright &copy; 2016 [Thomas Galliker](https://ch.linkedin.com/in/thomasgalliker). Free for non-commercial use. For commercial use please contact the author.

