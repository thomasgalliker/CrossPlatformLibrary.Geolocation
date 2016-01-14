using System;

using CrossPlatformLibrary.Geolocation.Exceptions;

using Guards;

namespace CrossPlatformLibrary.Geolocation
{
    public class PositionErrorEventArgs : EventArgs
    {
        public PositionErrorEventArgs(GeolocationException geolocationException)
        {
            Guard.ArgumentNotNull(geolocationException, "geolocationException");

            this.GeolocationException = geolocationException;
        }

        public GeolocationException GeolocationException { get; private set; }
    }
}