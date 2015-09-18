using System;
using System.Threading;
using System.Threading.Tasks;


using Guards;

namespace CrossPlatformLibrary.Geolocation
{
    internal class Timeout
    {
        private readonly CancellationTokenSource canceller = new CancellationTokenSource();

        public const int Infite = -1;

        public Timeout(int timeout, Action timesup)
        {
            if (timeout == Infite)
            {
                return; // nothing to do
            }
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }

            Guard.ArgumentNotNull(() => timesup);

            Task.Delay(TimeSpan.FromMilliseconds(timeout), this.canceller.Token).ContinueWith(
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