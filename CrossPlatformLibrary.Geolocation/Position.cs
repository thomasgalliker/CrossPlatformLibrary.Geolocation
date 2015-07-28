using System;

using Xamarin.Utils;

namespace CrossPlatformLibrary.Geolocation
{
    [System.Diagnostics.DebuggerDisplay("Longitude={Longitude}, Latitude={Latitude}")]
    public class Position
    {
        public static readonly Position Unknown = new Position();
        private double latitude = double.NaN;
        private double longitude = double.NaN;

        public Position()
        {
            this.IsUnknown = true;
        }

        public Position(Position position)
        {
            Guard.ArgumentNotNull(() => position);

            this.Timestamp = position.Timestamp;
            this.Latitude = position.Latitude;
            this.Longitude = position.Longitude;
            this.Altitude = position.Altitude;
            this.AltitudeAccuracy = position.AltitudeAccuracy;
            this.Accuracy = position.Accuracy;
            this.Heading = position.Heading;
            this.Speed = position.Speed;
        }

        public Position(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public bool IsUnknown { get; private set; }

        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        ///     Gets or sets the latitude.
        /// </summary>
        public double Latitude
        {
            get
            {
                return this.latitude;
            }
            set
            {
                this.latitude = value;
                this.CheckIfIsUnknown();
            }
        }

        /// <summary>
        ///     Gets or sets the longitude.
        /// </summary>
        public double Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.longitude = value;
                this.CheckIfIsUnknown();
            }
        }

        private void CheckIfIsUnknown()
        {
            if (double.IsNaN(this.longitude) || double.IsNaN(this.latitude))
            {
                this.IsUnknown = true;
            }
            else
            {
                this.IsUnknown = false;
            }
        }

        /// <summary>
        ///     Gets or sets the altitude in meters relative to sea level.
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        ///     Gets or sets the potential position error radius in meters.
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        ///     Gets or sets the potential altitude error range in meters.
        /// </summary>
        /// <remarks>
        ///     Not supported on Android, will always read 0.
        /// </remarks>
        public double AltitudeAccuracy { get; set; }

        /// <summary>
        ///     Gets or sets the heading in degrees relative to true North.
        /// </summary>
        public double Heading { get; set; }

        /// <summary>
        ///     Gets or sets the speed in meters per second.
        /// </summary>
        public double Speed { get; set; }

        public double GetDistanceTo(Position anotherPosition, DistanceUnit distanceUnit = DistanceUnit.Kilometers)
        {
            return this.CalculateDistance(
                this.Latitude,
                this.Longitude, 
                anotherPosition.Latitude,
                anotherPosition.Longitude,
                distanceUnit);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit)
        {
            double theta = lon1 - lon2;
            double distance = Math.Sin(DegreeToRadian(lat1)) * Math.Sin(DegreeToRadian(lat2)) + Math.Cos(DegreeToRadian(lat1)) * Math.Cos(DegreeToRadian(lat2)) * Math.Cos(DegreeToRadian(theta));
            distance = Math.Acos(distance);
            distance = RadianToDegree(distance);
            distance = distance * 60 * 1.1515;

            switch (unit)
            {
                case DistanceUnit.Kilometers:
                    distance = distance * 1.609344;
                    break;
                case DistanceUnit.NauticalMiles:
                    distance = distance * 0.8684;
                    break;
            }

            return distance;
        }

        private static double DegreeToRadian(double degree)
        {
            return (degree * Math.PI / 180.0);
        }

        private static double RadianToDegree(double radian)
        {
            return (radian / Math.PI * 180.0);
        }
    }
    public enum DistanceUnit
    {
        Kilometers,
        Miles,
        NauticalMiles
    }

    public class PositionEventArgs : EventArgs
    {
        public PositionEventArgs(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException("position");
            }

            this.Position = position;
        }

        public Position Position { get; private set; }
    }

    public class GeolocationException : Exception
    {
        public GeolocationException(GeolocationError error)
            : base("A geolocation error occured: " + error)
        {
            if (!Enum.IsDefined(typeof(GeolocationError), error))
            {
                throw new ArgumentException("error is not a valid GelocationError member", "error");
            }

            this.Error = error;
        }

        public GeolocationException(GeolocationError error, Exception innerException)
            : base("A geolocation error occured: " + error, innerException)
        {
            if (!Enum.IsDefined(typeof(GeolocationError), error))
            {
                throw new ArgumentException("error is not a valid GelocationError member", "error");
            }

            this.Error = error;
        }

        public GeolocationError Error { get; private set; }
    }

    public class PositionErrorEventArgs : EventArgs
    {
        public PositionErrorEventArgs(GeolocationError error)
        {
            this.Error = error;
        }

        public GeolocationError Error { get; private set; }
    }

    public enum GeolocationError
    {
        /// <summary>
        ///     The provider was unable to retrieve any position data.
        /// </summary>
        PositionUnavailable,

        /// <summary>
        ///     The app is not, or no longer, authorized to receive location data.
        /// </summary>
        Unauthorized
    }
}