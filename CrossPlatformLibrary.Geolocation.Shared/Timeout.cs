using System;
using System.Threading;
using System.Threading.Tasks;


using Guards;

namespace CrossPlatformLibrary.Geolocation
{
    public partial class Timeout
    {
        public const int Infinite = -1;

        private readonly CancellationTokenSource canceller = new CancellationTokenSource();

        public Timeout(int timeoutMilliseconds, Action timesup)
        {
            if (timeoutMilliseconds == Infinite)
            {
                return; // nothing to do
            }
            if (timeoutMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("timeoutMilliseconds");
            }

            Guard.ArgumentNotNull(() => timesup);

            Task.Delay(TimeSpan.FromMilliseconds(timeoutMilliseconds), this.canceller.Token).ContinueWith(
                t =>
                    {
                        if (!t.IsCanceled)
                        {
                            timesup();
                        }
                    });
        }

        public void Cancel()
        {
            this.canceller.Cancel();
        }
    }
}