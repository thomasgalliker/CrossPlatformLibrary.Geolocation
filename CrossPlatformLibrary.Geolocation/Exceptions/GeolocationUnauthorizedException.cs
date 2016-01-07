using System;

namespace CrossPlatformLibrary.Geolocation.Exceptions
{
    public class GeolocationUnauthorizedException : GeolocationException
    {
        private const string ErrorMessage = "A geolocation error occured: Unauthorized.";

        public GeolocationUnauthorizedException()
            : base(ErrorMessage)
        {
        }

        public GeolocationUnauthorizedException(Exception innerException)
            : base(ErrorMessage, innerException)
        {
        }
    }
}