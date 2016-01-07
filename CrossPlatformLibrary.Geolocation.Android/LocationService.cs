using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using CrossPlatformLibrary.Geolocation.Exceptions;
using CrossPlatformLibrary.Tracing;
using Guards;
using Java.Lang;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationService : ILocationService
    {
        private readonly ITracer tracer;

        private readonly string[] providers;
        private readonly LocationManager manager;

        private GeolocationContinuousListener listener;

        private readonly object positionSync = new object();
        private Position lastPosition;

        public LocationService(ITracer tracer)
        {
            this.DesiredAccuracy = 50;
            Guard.ArgumentNotNull(() => tracer);

            this.tracer = tracer;
            this.manager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
            this.providers = this.manager.GetProviders(enabledOnly: false).Where(s => s != LocationManager.PassiveProvider).ToArray();
        }

        /// <inheritdoc/>
        public event EventHandler<PositionErrorEventArgs> PositionError;

        /// <inheritdoc/>
        public event EventHandler<PositionEventArgs> PositionChanged;

        /// <inheritdoc/>
        public bool IsListening
        {
            get
            {
                return this.listener != null;
            }
        }

        /// <inheritdoc/>
        public double DesiredAccuracy { get; set; }

        /// <inheritdoc/>
        public bool SupportsHeading
        {
            get
            {
                return false;
                //				if (this.headingProvider == null || !this.manager.IsProviderEnabled (this.headingProvider))
                //				{
                //					Criteria c = new Criteria { BearingRequired = true };
                //					string providerName = this.manager.GetBestProvider (c, enabledOnly: false);
                //
                //					LocationProvider provider = this.manager.GetProvider (providerName);
                //
                //					if (provider.SupportsBearing())
                //					{
                //						this.headingProvider = providerName;
                //						return true;
                //					}
                //					else
                //					{
                //						this.headingProvider = null;
                //						return false;
                //					}
                //				}
                //				else
                //					return true;
            }
        }

        /// <inheritdoc/>
        public bool IsGeolocationAvailable
        {
            get
            {
                return this.providers.Length > 0;
            }
        }

        /// <inheritdoc/>
        public bool IsGeolocationEnabled
        {
            get
            {
                return this.providers.Any(this.manager.IsProviderEnabled);
            }
        }

        private bool CheckPermission(string permission)
        {
            var res = Application.Context.CheckCallingOrSelfPermission(permission);
            return (res == Permission.Granted);
        }

        /// <inheritdoc/>
        public Task<Position> GetPositionAsync(int timeoutMilliseconds = Timeout.Infinite, CancellationToken? cancelToken = null, bool includeHeading = false)
        {
            this.tracer.Debug("GetPositionAsync with timeout={0}, includeHeading={1}", timeoutMilliseconds, includeHeading);

            if (!this.CheckPermission("android.permission.ACCESS_COARSE_LOCATION"))
            {
                Console.WriteLine("Unable to get location, ACCESS_COARSE_LOCATION not set.");
                return null;
            }

            if (!this.CheckPermission("android.permission.ACCESS_FINE_LOCATION"))
            {
                Console.WriteLine("Unable to get location, ACCESS_FINE_LOCATION not set.");
                return null;
            }

            if (timeoutMilliseconds <= 0 && timeoutMilliseconds != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException("timeoutMilliseconds", "timeout must be greater than or equal to 0");
            }

            if (!cancelToken.HasValue)
            {
                cancelToken = CancellationToken.None;
            }

            var tcs = new TaskCompletionSource<Position>();

            if (!this.IsListening)
            {
                GeolocationSingleListener singleListener = null;
                singleListener = new GeolocationSingleListener(
                    (float)this.DesiredAccuracy,
                    timeoutMilliseconds,
                    this.providers.Where(this.manager.IsProviderEnabled),
                    finishedCallback: () =>
                        {
                            for (int i = 0; i < this.providers.Length; ++i)
                            {
                                this.manager.RemoveUpdates(singleListener);
                            }
                        });

                if (cancelToken != CancellationToken.None)
                {
                    cancelToken.Value.Register(
                        () =>
                            {
                                singleListener.Cancel();

                                for (int i = 0; i < this.providers.Length; ++i)
                                {
                                    this.manager.RemoveUpdates(singleListener);
                                }
                            },
                        true);
                }

                try
                {
                    Looper looper = Looper.MyLooper() ?? Looper.MainLooper;

                    int enabled = 0;
                    for (int i = 0; i < this.providers.Length; ++i)
                    {
                        if (this.manager.IsProviderEnabled(this.providers[i]))
                        {
                            enabled++;
                        }

                        this.manager.RequestLocationUpdates(this.providers[i], 0, 0, singleListener, looper);
                    }

                    if (enabled == 0)
                    {
                        for (int i = 0; i < this.providers.Length; ++i)
                        {
                            this.manager.RemoveUpdates(singleListener);
                        }

                        tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
                        return tcs.Task;
                    }
                }
                catch (SecurityException ex)
                {
                    tcs.SetException(new GeolocationException(GeolocationError.Unauthorized, ex));
                    return tcs.Task;
                }

                var singlePositionListenerTask = singleListener.Task;
                singlePositionListenerTask.ContinueWith((callback) =>
                {
                    if (callback.Status == TaskStatus.RanToCompletion)
                    {
                        this.RaisePositionChangedEvent(callback.Result);
                    }
                });
                return singlePositionListenerTask;
            }

            // If we're already listening, just use the current listener
            lock (this.positionSync)
            {
                if (this.lastPosition == null)
                {
                    if (cancelToken != CancellationToken.None)
                    {
                        cancelToken.Value.Register(() => tcs.TrySetCanceled());
                    }

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
                    tcs.SetResult(this.lastPosition);
                }
            }

            return tcs.Task;
        }

        /// <inheritdoc/>
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
                throw new InvalidOperationException("This LocationService is already listening");
            }

            this.listener = new GeolocationContinuousListener(this.manager, TimeSpan.FromMilliseconds(minTime), this.providers);
            this.listener.PositionChanged += this.OnListenerPositionChanged;
            this.listener.PositionError += this.OnListenerPositionError;

            Looper looper = Looper.MyLooper() ?? Looper.MainLooper;
            for (int i = 0; i < this.providers.Length; ++i)
            {
                this.manager.RequestLocationUpdates(this.providers[i], minTime, (float)minDistance, this.listener, looper);
            }
        }

        /// <inheritdoc/>
        public void StopListening()
        {
            if (this.listener == null)
            {
                return;
            }

            this.listener.PositionChanged -= this.OnListenerPositionChanged;
            this.listener.PositionError -= this.OnListenerPositionError;

            for (int i = 0; i < this.providers.Length; ++i)
            {
                this.manager.RemoveUpdates(this.listener);
            }

            this.listener = null;
        }

        private void OnListenerPositionChanged(object sender, PositionEventArgs e)
        {
            if (!this.IsListening) // ignore anything that might come in afterwards
            {
                return;
            }

            lock (this.positionSync)
            {
                this.lastPosition = e.Position;
                this.RaisePositionChangedEvent(e.Position);
            }
        }

        private void RaisePositionChangedEvent(Position position)
        {
            var positionChangedHandler = this.PositionChanged;
            if (positionChangedHandler != null)
            {
                positionChangedHandler(this, new PositionEventArgs(position));
            }
        }

        private void OnListenerPositionError(object sender, PositionErrorEventArgs e)
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