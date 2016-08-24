using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationService : ILocationService
    {
        public event EventHandler<PositionErrorEventArgs> PositionError;

        public event EventHandler<PositionEventArgs> PositionChanged;

        public double DesiredAccuracy
        {
            get
            {
                throw new NotImplementedInReferenceAssemblyException();
            }
            set
            {
                throw new NotImplementedInReferenceAssemblyException();
            }
        }

        public bool IsListening
        {
            get
            {
                throw new NotImplementedInReferenceAssemblyException();
            }
        }

        public bool SupportsHeading
        {
            get
            {
                throw new NotImplementedInReferenceAssemblyException();
            }
        }

        public bool IsGeolocationAvailable
        {
            get
            {
                throw new NotImplementedInReferenceAssemblyException();
            }
        }

        public bool IsGeolocationEnabled
        {
            get
            {
                throw new NotImplementedInReferenceAssemblyException();
            }
        }

        public Task<Position> GetPositionAsync(int timeoutMilliseconds = Timeout.Infinite, CancellationToken? token = null, bool includeHeading = false)
        {
            throw new NotImplementedInReferenceAssemblyException();
        }

        public void StartListening(int minTime, double minDistance, bool includeHeading = false)
        {
            throw new NotImplementedInReferenceAssemblyException();
        }

        public void StopListening()
        {
            throw new NotImplementedInReferenceAssemblyException();
        }
    }
}
