using System;

namespace CrossPlatformLibrary.Geolocation.Exceptions
{
    public abstract class GeolocationException : Exception
    {
        protected GeolocationException(string message)
            : base(message)
        {
        }

        protected GeolocationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}