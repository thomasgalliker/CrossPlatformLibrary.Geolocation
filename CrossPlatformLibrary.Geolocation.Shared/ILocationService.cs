using CrossPlatformLibrary.Geolocation.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrossPlatformLibrary.Geolocation
{
    public interface ILocationService
    {
        event EventHandler<PositionErrorEventArgs> PositionError;

        event EventHandler<PositionEventArgs> PositionChanged;

        /// <summary>
        /// Desired accuracy in meteres.
        /// The better accuracy (=the lower values), the longer it takes until the GPS sensor returns a position.
        /// </summary>
        double DesiredAccuracy { get; set; }

        bool IsListening { get; }

        bool SupportsHeading { get; }

        /// <summary>
        /// Indicates if the geolocation service is available on this device.
        /// </summary>
        bool IsGeolocationAvailable { get; }

        /// <summary>
        /// Indicates if the geolocation service is enabled on this device.
        /// </summary>
        bool IsGeolocationEnabled { get; }

        /// <summary>
        /// Gets position async with specified parameters
        /// </summary>
        /// <param name="timeoutMilliseconds">Timeout in milliseconds to wait, Default Infinite</param>
        /// <param name="token">Cancelation token</param>
        /// <param name="includeHeading">If you would like to include heading</param>
        /// <returns>Position</returns>
        /// <exception cref="TaskCanceledException">Thrown when the request times out.</exception>
        /// <exception cref="GeolocationPositionUnavailableException">Thrown when no valid GPS position could be found.</exception>
        /// <exception cref="GeolocationUnauthorizedException">Thrown when the application has no permissions to use the GPS service.</exception>
        Task<Position> GetPositionAsync(int timeoutMilliseconds = Timeout.Infinite, CancellationToken? token = null, bool includeHeading = false);

        /// <summary>
        /// Starts listening for GPS position updates.
        /// </summary>
        /// <param name="minTime">The requested minimum time interval between location updates, in milliseconds. If your application requires updates infrequently, set this value so that location services can conserve power by calculating location only when needed.</param>
        /// <param name="minDistance">Gets and sets the distance of movement, in meters, relative to the coordinate from the last PositionChanged event, that is required for the location service to raise a PositionChanged event.</param>
        /// <param name="includeHeading"></param>
        void StartListening(int minTime, double minDistance, bool includeHeading = false);

        /// <summary>
        /// Stops listening for GPS position updates.
        /// </summary>
        void StopListening();
    }
}