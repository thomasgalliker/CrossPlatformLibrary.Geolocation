using System;

namespace CrossPlatformLibrary.Geolocation.Exceptions
{
    public class GeolocationPositionUnavailableException : GeolocationException
    {
        public GeolocationPositionUnavailableException()
            : base("A geolocation error occured: Position unavailable.")
        {
        }
    }
}