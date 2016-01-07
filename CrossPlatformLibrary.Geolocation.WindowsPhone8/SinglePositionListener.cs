using CrossPlatformLibrary.Geolocation.Exceptions;
using System;
using System.Device.Location;
using System.Threading;
using System.Threading.Tasks;

namespace CrossPlatformLibrary.Geolocation
{
    internal class SinglePositionListener
    {
        private GeoPosition<GeoCoordinate> bestPosition;
        private GeoCoordinateWatcher watcher;
        private readonly double desiredAccuracy;
        private readonly DateTimeOffset start;
        private readonly Timer timer;
        private readonly int timeoutMilliseconds;
        private readonly TaskCompletionSource<Position> tcs = new TaskCompletionSource<Position>();
        private readonly CancellationToken cancelToken;

        internal SinglePositionListener(double accuracy, int timeoutMilliseconds, CancellationToken cancelToken)
        {
            cancelToken.Register(this.HandleTimeout, true);
            this.cancelToken = cancelToken;
            this.desiredAccuracy = accuracy;
            this.start = DateTime.Now;
            this.timeoutMilliseconds = timeoutMilliseconds;

            System.Threading.Tasks.Task.Factory.StartNew(
                () =>
                    {
                        this.watcher = new GeoCoordinateWatcher(LocationService.GetAccuracy(accuracy));
                        this.watcher.PositionChanged += this.WatcherOnPositionChanged;
                        this.watcher.StatusChanged += this.WatcherOnStatusChanged;

                        this.watcher.Start();
                    });

            if (timeoutMilliseconds != Timeout.Infinite)
            {
                this.timer = new Timer(this.HandleTimeout, null, timeoutMilliseconds, Timeout.Infinite);
            }

            this.Task.ContinueWith(this.Cleanup);
        }

        public Task<Position> Task
        {
            get
            {
                return this.tcs.Task;
            }
        }

        private void Cleanup(Task task)
        {
            this.watcher.PositionChanged -= this.WatcherOnPositionChanged;
            this.watcher.StatusChanged -= this.WatcherOnStatusChanged;

            this.watcher.Stop();
            this.watcher.Dispose();

            if (this.timer != null)
            {
                this.timer.Dispose();
            }
        }

        private void HandleTimeout(object state)
        {
            if (state != null && (bool)state)
            {
                this.tcs.TrySetCanceled();
            }

            if (this.bestPosition != null)
            {
                this.tcs.TrySetResult(LocationService.GetPosition(this.bestPosition));
            }
            else
            {
                this.tcs.TrySetCanceled();
            }
        }

        private void WatcherOnStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.NoData:
                    this.tcs.TrySetException(new GeolocationException(GeolocationError.PositionUnavailable));
                    break;

                case GeoPositionStatus.Disabled:
                    this.tcs.TrySetException(new GeolocationException(GeolocationError.Unauthorized));
                    break;
            }
        }

        private void WatcherOnPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position.Location.IsUnknown)
            {
                return;
            }

            bool isRecent = (e.Position.Timestamp - this.start).TotalMilliseconds < this.timeoutMilliseconds || (this.timeoutMilliseconds == Timeout.Infinite && this.cancelToken == CancellationToken.None);

            if (e.Position.Location.HorizontalAccuracy <= this.desiredAccuracy && isRecent)
            {
                this.tcs.TrySetResult(LocationService.GetPosition(e.Position));
            }

            if (this.bestPosition == null || e.Position.Location.HorizontalAccuracy < this.bestPosition.Location.HorizontalAccuracy)
            {
                this.bestPosition = e.Position;
            }
        }
    }
}