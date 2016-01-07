using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Android.Locations;
using Android.OS;

using CrossPlatformLibrary.Geolocation.Exceptions;

using Object = Java.Lang.Object;

namespace CrossPlatformLibrary.Geolocation
{
    internal class GeolocationSingleListener : Object, ILocationListener
    {
        public GeolocationSingleListener(float desiredAccuracy, int timeoutMilliseconds, IEnumerable<string> activeProviders, Action finishedCallback)
        {
            this.desiredAccuracy = desiredAccuracy;
            this.finishedCallback = finishedCallback;

            this.activeProviders = new HashSet<string>(activeProviders);

            if (timeoutMilliseconds != Timeout.Infinite)
            {
                this.timer = new Timer(this.TimesUp, null, timeoutMilliseconds, 0);
            }
        }

        public Task<Position> Task
        {
            get
            {
                return this.completionSource.Task;
            }
        }

        public void OnLocationChanged(Location location)
        {
            if (location.Accuracy <= this.desiredAccuracy)
            {
                this.Finish(location);
                return;
            }

            lock (this.locationSync)
            {
                if (this.bestLocation == null || location.Accuracy <= this.bestLocation.Accuracy)
                {
                    this.bestLocation = location;
                }
            }
        }

        public void OnProviderDisabled(string provider)
        {
            lock (this.activeProviders)
            {
                if (this.activeProviders.Remove(provider) && this.activeProviders.Count == 0)
                {
                    this.completionSource.TrySetException(new GeolocationException(GeolocationError.PositionUnavailable));
                }
            }
        }

        public void OnProviderEnabled(string provider)
        {
            lock (this.activeProviders) this.activeProviders.Add(provider);
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            switch (status)
            {
                case Availability.Available:
                    this.OnProviderEnabled(provider);
                    break;

                case Availability.OutOfService:
                    this.OnProviderDisabled(provider);
                    break;
            }
        }

        public void Cancel()
        {
            this.completionSource.TrySetCanceled();
        }

        private readonly object locationSync = new object();
        private Location bestLocation;

        private readonly Action finishedCallback;
        private readonly float desiredAccuracy;
        private readonly Timer timer;
        private readonly TaskCompletionSource<Position> completionSource = new TaskCompletionSource<Position>();
        private readonly HashSet<string> activeProviders = new HashSet<string>();

        private void TimesUp(object state)
        {
            lock (this.locationSync)
            {
                if (this.bestLocation == null)
                {
                    if (this.completionSource.TrySetCanceled() && this.finishedCallback != null)
                    {
                        this.finishedCallback();
                    }
                }
                else
                {
                    this.Finish(this.bestLocation);
                }
            }
        }

        private void Finish(Location location)
        {
            var position = new Position();
            if (location.HasAccuracy)
            {
                position.Accuracy = location.Accuracy;
            }
            if (location.HasAltitude)
            {
                position.Altitude = location.Altitude;
            }
            if (location.HasBearing)
            {
                position.Heading = location.Bearing;
            }
            if (location.HasSpeed)
            {
                position.Speed = location.Speed;
            }

            position.Longitude = location.Longitude;
            position.Latitude = location.Latitude;
            position.Timestamp = DateTimeExtensions.GetTimestamp(location.Time);

            if (this.finishedCallback != null)
            {
                this.finishedCallback();
            }

            this.completionSource.TrySetResult(position);
        }
    }
}