using CrossPlatformLibrary.Geolocation.Extensions;

using System;
using System.Threading;
using System.Threading.Tasks;

using CrossPlatformLibrary.Geolocation.Exceptions;
using CrossPlatformLibrary.Tracing;

using Guards;
#if __UNIFIED__
using CoreLocation;
using Foundation;
using UIKit;
#else
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

#endif

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationService : ILocationService
    {
        private readonly ITracer tracer;

        private readonly CLLocationManager manager;
        private Position position;

        public LocationService(ITracer tracer)
        {
            this.DesiredAccuracy = 100;
            Guard.ArgumentNotNull(() => tracer);

            this.tracer = tracer;

            this.manager = GetLocationManager();
            this.manager.AuthorizationChanged += this.OnAuthorizationChanged;
            this.manager.Failed += this.OnFailed;

            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
            {
                this.manager.LocationsUpdated += this.OnLocationsUpdated;
            }
            else
            {
                this.manager.UpdatedLocation += this.OnUpdatedLocation;
            }

            this.manager.UpdatedHeading += this.OnUpdatedHeading;
            this.manager.RequestAuthorization();
        }

        /// <inheritdoc />
        public event EventHandler<PositionErrorEventArgs> PositionError;

        /// <inheritdoc />
        public event EventHandler<PositionEventArgs> PositionChanged;

        /// <inheritdoc />
        public double DesiredAccuracy { get; set; }

        /// <inheritdoc />
        public bool IsListening { get; private set; }

        /// <inheritdoc />
        public bool SupportsHeading
        {
            get
            {
                return CLLocationManager.HeadingAvailable;
            }
        }

        /// <inheritdoc />
        public bool IsGeolocationAvailable
        {
            get
            {
                return true;
            } // all iOS devices support at least wifi geolocation
        }

        private bool IsAuthorized
        {
            get
            {
                var status = CLLocationManager.Status;

                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    return status == CLAuthorizationStatus.AuthorizedAlways
                        || status == CLAuthorizationStatus.AuthorizedWhenInUse;
                }

                return status == CLAuthorizationStatus.Authorized;
            }
        }

        /// <inheritdoc />
        public bool IsGeolocationEnabled
        {
            get
            {
                return CLLocationManager.LocationServicesEnabled && this.IsAuthorized;
            }
        }

        /// <inheritdoc />
        public Task<Position> GetPositionAsync(int timeoutMilliseconds = Timeout.Infinite, CancellationToken? cancelToken = null, bool includeHeading = false)
        {
            this.tracer.Debug("GetPositionAsync with timeoutMilliseconds={0}, includeHeading={1}", timeoutMilliseconds, includeHeading);

            if (timeoutMilliseconds <= 0 && timeoutMilliseconds != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException("timeoutMilliseconds", "Timeout must be positive or Timeout.Infinite");
            }

            if (!cancelToken.HasValue)
            {
                cancelToken = CancellationToken.None;
            }
            ;

            if (!this.IsListening)
            {
                var m = GetLocationManager();

                var singleListener = new GeolocationSingleUpdateDelegate(m, this.DesiredAccuracy, includeHeading, timeoutMilliseconds, cancelToken.Value);
                m.Delegate = singleListener;

                m.StartUpdatingLocation();
                if (includeHeading && this.SupportsHeading)
                {
                    m.StartUpdatingHeading();
                }

                return singleListener.Task;
            }

            var tcs = new TaskCompletionSource<Position>();
            if (this.position == null)
            {
                EventHandler<PositionErrorEventArgs> gotError = null;
                gotError = (s, e) =>
                    {
                        if (e.Error == GeolocationError.Unauthorized)
                        {
                            tcs.TrySetException(new GeolocationUnauthorizedException());
                        }
                        else if (e.Error == GeolocationError.PositionUnavailable)
                        {
                            tcs.TrySetException(new GeolocationPositionUnavailableException());
                        }
                        else
                        {
                            tcs.TrySetException(new Exception(string.Format("Unknown PositionErrorEventArgs: {0}", e.Error)));
                        }

                        this.PositionError -= gotError;
                    };

                this.PositionError += gotError;

                EventHandler<PositionEventArgs> gotPosition = null;
                gotPosition = (s, e) =>
                    {
                        tcs.TrySetResult(e.Position);
                        this.PositionChanged -= gotPosition;
                    };

                this.PositionChanged += gotPosition;
            }
            else
            {
                tcs.SetResult(this.position);
            }

            return tcs.Task;
        }

        /// <inheritdoc />
        public void StartListening(int minTime, double minDistance, bool includeHeading = false)
        {
            if (minTime < 0)
            {
                throw new ArgumentOutOfRangeException("minTime");
            }
            if (minDistance < 0)
            {
                throw new ArgumentOutOfRangeException("minDistance");
            }
            if (this.IsListening)
            {
                throw new InvalidOperationException("Already listening");
            }

            this.IsListening = true;
            this.manager.DesiredAccuracy = this.DesiredAccuracy;
            this.manager.DistanceFilter = minDistance;
            this.manager.StartUpdatingLocation();

            if (includeHeading && CLLocationManager.HeadingAvailable)
            {
                this.manager.StartUpdatingHeading();
            }
        }

        /// <inheritdoc />
        public void StopListening()
        {
            if (!this.IsListening)
            {
                return;
            }

            this.IsListening = false;
            if (CLLocationManager.HeadingAvailable)
            {
                this.manager.StopUpdatingHeading();
            }

            this.manager.StopUpdatingLocation();
            this.position = null;
        }

        internal static CLLocationManager GetLocationManager()
        {
            CLLocationManager m = null;
            new NSObject().InvokeOnMainThread(() => m = new CLLocationManager());
            return m;
        }

        private void OnUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
        {
            if (e.NewHeading.TrueHeading == -1)
            {
                return;
            }

            Position p = (this.position == null) ? new Position() : new Position(this.position);

            p.Heading = e.NewHeading.TrueHeading;

            this.position = p;

            this.OnPositionChanged(new PositionEventArgs(p));
        }

        private void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            foreach (CLLocation location in e.Locations)
            {
                this.UpdatePosition(location);
            }
        }

        private void OnUpdatedLocation(object sender, CLLocationUpdatedEventArgs e)
        {
            this.UpdatePosition(e.NewLocation);
        }

        private void UpdatePosition(CLLocation location)
        {
            Position p = (this.position == null) ? new Position() : new Position(this.position);

            if (location.HorizontalAccuracy > -1)
            {
                p.Accuracy = location.HorizontalAccuracy;
                p.Latitude = location.Coordinate.Latitude;
                p.Longitude = location.Coordinate.Longitude;
            }

            if (location.VerticalAccuracy > -1)
            {
                p.Altitude = location.Altitude;
                p.AltitudeAccuracy = location.VerticalAccuracy;
            }

            if (location.Speed > -1)
            {
                p.Speed = location.Speed;
            }

            var dateTime = DateTime.SpecifyKind(location.Timestamp.ToDateTime(), DateTimeKind.Unspecified);
            p.Timestamp = new DateTimeOffset(dateTime);

            this.position = p;

            this.OnPositionChanged(new PositionEventArgs(p));

            location.Dispose();
        }

        private void OnFailed(object sender, NSErrorEventArgs e)
        {
            if ((CLError)(int)e.Error.Code == CLError.Network)
            {
                this.OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
            }
        }

        private void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
        {
            if (e.Status == CLAuthorizationStatus.Denied || e.Status == CLAuthorizationStatus.Restricted)
            {
                this.OnPositionError(new PositionErrorEventArgs(GeolocationError.Unauthorized));
            }
        }

        private void OnPositionChanged(PositionEventArgs e)
        {
            var changed = this.PositionChanged;
            if (changed != null)
            {
                changed(this, e);
            }
        }

        private void OnPositionError(PositionErrorEventArgs e)
        {
            this.StopListening();

            var error = this.PositionError;
            if (error != null)
            {
                error(this, e);
            }
        }
    }
}