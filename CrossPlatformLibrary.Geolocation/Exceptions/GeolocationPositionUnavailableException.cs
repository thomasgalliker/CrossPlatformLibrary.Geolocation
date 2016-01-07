using System;

namespace CrossPlatformLibrary.Geolocation.Exceptions
{
    public class GeolocationPositionUnavailableException : Exception
    {
        public GeolocationPositionUnavailableException()
            : base("A geolocation error occured: Position unavailable.")
        {
        }
    }
}