using System;

using CrossPlatformLibrary.Geolocation.Exceptions;

using Guards;

namespace CrossPlatformLibrary.Geolocation
{
    public class PositionErrorEventArgs : EventArgs
    {
        public PositionErrorEventArgs(GeolocationError error)
        {
            Guard.ArgumentNotNull(() => error);

            this.Error = error;
        }

        public GeolocationError Error { get; private set; }
    }
}