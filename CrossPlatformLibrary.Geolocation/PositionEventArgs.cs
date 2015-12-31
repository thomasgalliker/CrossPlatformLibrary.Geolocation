using System;

using Guards;

namespace CrossPlatformLibrary.Geolocation
{
    public class PositionEventArgs : EventArgs
    {
        public PositionEventArgs(Position position)
        {
            Guard.ArgumentNotNull(position, "position");

            this.Position = position;
        }

        public Position Position { get; private set; }
    }
}