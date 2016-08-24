using System;
using System.Diagnostics;


using Guards;

namespace CrossPlatformLibrary.Geolocation
{
    [DebuggerDisplay("Longitude={Longitude}, Latitude={Latitude}")]
    public class Position
    {
        public static readonly Position Unknown = new Position();

        public Position()
        {
            this.Latitude = double.NaN;
            this.Longitude = double.NaN;
        }

        public Position(Position position)
        {
            Guard.ArgumentNotNull(position, "position");

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

        public bool IsUnknown
        {
            get
            {
                if (double.IsNaN(this.Longitude) || double.IsNaN(this.Latitude))
                {
                    return true;
                }
                
                return false;
            }
        }

        public DateTimeOffset Timestamp { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

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
}