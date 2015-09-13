using System;

using CrossPlatformLibrary.Utils;

namespace CrossPlatformLibrary.Geolocation
{
    public class PositionEventArgs : EventArgs
    {
        public PositionEventArgs(Position position)
        {
            Guard.ArgumentNotNull(() => position);

            this.Position = position;
        }

        public Position Position { get; private set; }
    }
}