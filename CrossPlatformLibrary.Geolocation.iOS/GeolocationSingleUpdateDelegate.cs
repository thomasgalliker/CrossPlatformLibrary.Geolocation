using System;
using System.Threading;
using System.Threading.Tasks;

using CrossPlatformLibrary.Geolocation.Exceptions;
#if __UNIFIED__
using CoreLocation;
using Foundation;
#else
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
#endif

namespace CrossPlatformLibrary.Geolocation
{
    internal class GeolocationSingleUpdateDelegate : CLLocationManagerDelegate
    {
        public GeolocationSingleUpdateDelegate(CLLocationManager manager, double desiredAccuracy, bool includeHeading, int timeoutMilliseconds, CancellationToken cancelToken)
        {
            this.manager = manager;
            this.tcs = new TaskCompletionSource<Position>(manager);
            this.desiredAccuracy = desiredAccuracy;
            this.includeHeading = includeHeading;

            if (timeoutMilliseconds != Timeout.Infinite)
            {
                Timer t = null;
                t = new Timer(
                    s =>
                        {
                            if (this.haveLocation)
                            {
                                this.tcs.TrySetResult(new Position(this.position));
                            }
                            else
                            {
                                this.tcs.TrySetCanceled();
                            }

                            this.StopListening();
                            t.Dispose();
                        },
                    null,
                    timeoutMilliseconds,
                    0);
            }

            cancelToken.Register(
                () =>
                    {
                        this.StopListening();
                        this.tcs.TrySetCanceled();
                    });
        }

        public Task<Position> Task
        {
            get
            {
                return this.tcs.Task;
            }
        }

        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            // If user has services disabled, we're just going to throw an exception for consistency.
            if (status == CLAuthorizationStatus.Denied || status == CLAuthorizationStatus.Restricted)
            {
                this.StopListening();
                this.tcs.TrySetException(new GeolocationException(GeolocationError.Unauthorized));
            }
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            switch ((CLError)(int)error.Code)
            {
                case CLError.Network:
                    this.StopListening();
                    this.tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
                    break;
            }
        }

        public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager)
        {
            return true;
        }

        public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
        {
            if (newLocation.HorizontalAccuracy < 0)
            {
                return;
            }

            if (this.haveLocation && newLocation.HorizontalAccuracy > this.position.Accuracy)
            {
                return;
            }

            this.position.Accuracy = newLocation.HorizontalAccuracy;
            this.position.Altitude = newLocation.Altitude;
            this.position.AltitudeAccuracy = newLocation.VerticalAccuracy;
            this.position.Latitude = newLocation.Coordinate.Latitude;
            this.position.Longitude = newLocation.Coordinate.Longitude;
            this.position.Speed = newLocation.Speed;
            this.position.Timestamp = new DateTimeOffset(newLocation.Timestamp.ToDateTime());

            this.haveLocation = true;

            if ((!this.includeHeading || this.haveHeading) && this.position.Accuracy <= this.desiredAccuracy)
            {
                this.tcs.TrySetResult(new Position(this.position));
                this.StopListening();
            }
        }

        public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
        {
            if (newHeading.HeadingAccuracy < 0)
            {
                return;
            }
            if (this.bestHeading != null && newHeading.HeadingAccuracy >= this.bestHeading.HeadingAccuracy)
            {
                return;
            }

            this.bestHeading = newHeading;
            this.position.Heading = newHeading.TrueHeading;
            this.haveHeading = true;

            if (this.haveLocation && this.position.Accuracy <= this.desiredAccuracy)
            {
                this.tcs.TrySetResult(new Position(this.position));
                this.StopListening();
            }
        }

        private bool haveHeading;
        private bool haveLocation;
        private readonly Position position = new Position();
        private CLHeading bestHeading;

        private readonly double desiredAccuracy;
        private readonly bool includeHeading;
        private readonly TaskCompletionSource<Position> tcs;
        private readonly CLLocationManager manager;

        private void StopListening()
        {
            if (CLLocationManager.HeadingAvailable)
            {
                this.manager.StopUpdatingHeading();
            }

            this.manager.StopUpdatingLocation();
        }
    }
}