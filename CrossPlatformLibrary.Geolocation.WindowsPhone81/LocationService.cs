using CrossPlatformLibrary.Geolocation.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationService : ILocationService
    {
        public LocationService()
        {
            this.DesiredAccuracy = 100;
        }

        /// <inheritdoc />
        public event EventHandler<PositionEventArgs> PositionChanged;

        /// <inheritdoc />
        public event EventHandler<PositionErrorEventArgs> PositionError;

        /// <inheritdoc />
        public bool SupportsHeading
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool IsGeolocationAvailable
        {
            get
            {
                PositionStatus status = this.GetGeolocatorStatus();

                while (status == PositionStatus.Initializing)
                {
                    Task.Delay(10).Wait();
                    status = this.GetGeolocatorStatus();
                }

                return status != PositionStatus.NotAvailable;
            }
        }

        /// <inheritdoc />
        public bool IsGeolocationEnabled
        {
            get
            {
                PositionStatus status = this.GetGeolocatorStatus();

                while (status == PositionStatus.Initializing)
                {
                    Task.Delay(10).Wait();
                    status = this.GetGeolocatorStatus();
                }

                return status != PositionStatus.Disabled && status != PositionStatus.NotAvailable;
            }
        }

        /// <inheritdoc />
        public double DesiredAccuracy
        {
            get
            {
                return this.desiredAccuracy;
            }
            set
            {
                this.desiredAccuracy = value;
                this.GetGeolocator().DesiredAccuracy = (value < 100) ? PositionAccuracy.High : PositionAccuracy.Default;
            }
        }

        /// <inheritdoc />
        public bool IsListening { get; private set; }

        /// <inheritdoc />
        public Task<Position> GetPositionAsync(int timeoutMilliseconds = Timeout.Infite, CancellationToken? token = null, bool includeHeading = false)
        {
            if (timeoutMilliseconds < 0 && timeoutMilliseconds != Timeout.Infite)
            {
                throw new ArgumentOutOfRangeException("timeoutMilliseconds");
            }

            if (!token.HasValue)
            {
                token = CancellationToken.None;
            }

            IAsyncOperation<Geoposition> pos = this.GetGeolocator().GetGeopositionAsync(TimeSpan.FromTicks(0), TimeSpan.FromDays(365));
            token.Value.Register(o => ((IAsyncOperation<Geoposition>)o).Cancel(), pos);

            var timer = new Timeout(timeoutMilliseconds, pos.Cancel);

            var tcs = new TaskCompletionSource<Position>();

            pos.Completed = (op, s) =>
                {
                    timer.Cancel();

                    switch (s)
                    {
                        case AsyncStatus.Canceled:
                            tcs.SetCanceled();
                            break;
                        case AsyncStatus.Completed:
                            tcs.SetResult(GetPosition(op.GetResults()));
                            break;
                        case AsyncStatus.Error:
                            Exception ex = op.ErrorCode;
                            if (ex is UnauthorizedAccessException)
                            {
                                ex = new GeolocationUnauthorizedException(ex);
                            }

                            tcs.SetException(ex);
                            break;
                    }
                };

            return tcs.Task;
        }

        /// <inheritdoc />
        public void StartListening(int minTime, double minDistance, bool includeHeading = false)
        {
            if (minTime < 0)
            {
                throw new ArgumentOutOfRangeException("minTime");
            }
            if (minTime < minDistance)
            {
                throw new ArgumentOutOfRangeException("minDistance");
            }
            if (this.IsListening)
            {
                throw new InvalidOperationException();
            }

            this.IsListening = true;

            var loc = this.GetGeolocator();
            loc.ReportInterval = (uint)minTime;
            loc.MovementThreshold = minDistance;
            loc.PositionChanged += this.OnLocatorPositionChanged;
            loc.StatusChanged += this.OnLocatorStatusChanged;
        }

        /// <inheritdoc />
        public void StopListening()
        {
            if (!this.IsListening)
            {
                return;
            }

            this.locator.PositionChanged -= this.OnLocatorPositionChanged;
            this.IsListening = false;
        }

        private double desiredAccuracy;
        private Geolocator locator = new Geolocator();

        private void OnLocatorStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            GeolocationException geolocationException;
            switch (e.Status)
            {
                case PositionStatus.Disabled:
                    geolocationException = new GeolocationUnauthorizedException();
                    break;

                case PositionStatus.NoData:
                    geolocationException = new GeolocationPositionUnavailableException();
                    break;

                default:
                    return;
            }

            if (this.IsListening)
            {
                this.StopListening();
                this.OnPositionError(new PositionErrorEventArgs(geolocationException));
            }

            this.locator = null;
        }

        private void OnLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            this.OnPositionChanged(new PositionEventArgs(GetPosition(e.Position)));
        }

        private void OnPositionChanged(PositionEventArgs e)
        {
            var handler = this.PositionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPositionError(PositionErrorEventArgs e)
        {
            var handler = this.PositionError;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private Geolocator GetGeolocator()
        {
            var loc = this.locator;
            if (loc == null)
            {
                this.locator = new Geolocator();
                this.locator.StatusChanged += this.OnLocatorStatusChanged;
                loc = this.locator;
            }

            return loc;
        }

        private PositionStatus GetGeolocatorStatus()
        {
            var loc = this.GetGeolocator();
            return loc.LocationStatus;
        }

        private static Position GetPosition(Geoposition position)
        {
            var pos = new Position
            {
                Accuracy = position.Coordinate.Accuracy,
                Latitude = position.Coordinate.Point.Position.Latitude,
                Longitude = position.Coordinate.Point.Position.Longitude,
                Timestamp = position.Coordinate.Timestamp.ToUniversalTime(),
            };

            if (position.Coordinate.Heading != null)
            {
                pos.Heading = position.Coordinate.Heading.Value;
            }

            if (position.Coordinate.Speed != null)
            {
                pos.Speed = position.Coordinate.Speed.Value;
            }

            if (position.Coordinate.AltitudeAccuracy.HasValue)
            {
                pos.AltitudeAccuracy = position.Coordinate.AltitudeAccuracy.Value;
            }

            pos.Altitude = position.Coordinate.Point.Position.Altitude;

            return pos;
        }
    }
}