namespace CrossPlatformLibrary.Geolocation.Exceptions
{
    public enum GeolocationError
    {
        /// <summary>
        ///     The provider was unable to retrieve any position data.
        /// </summary>
        PositionUnavailable,

        /// <summary>
        ///     The app is not, or no longer, authorized to receive location data.
        /// </summary>
        Unauthorized
    }
}