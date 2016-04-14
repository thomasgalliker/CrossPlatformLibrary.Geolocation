using System;
using System.Collections.Generic;

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationRect : IFormattable
    {
        private const double MaxLatitude = 90.0;
        private const double MinLatitude = -90.0;
        private const double MaxLongitude = 180.0;
        private const double MinLongitude = -180.0;
        private double halfHeight;
        private double halfWidth;

        public double North
        {
            get
            {
                return this.Center.Latitude + this.halfHeight;
            }
            set
            {
                this.Init(value, this.West, this.South, this.East);
            }
        }

        public double West
        {
            get
            {
                if (this.halfWidth != 180.0)
                {
                    return NormalizeLongitude(this.Center.Longitude - this.halfWidth);
                }
                return -180.0;
            }
            set
            {
                this.Init(this.North, value, this.South, this.East);
            }
        }

        public double South
        {
            get
            {
                return this.Center.Latitude - this.halfHeight;
            }
            set
            {
                this.Init(this.North, this.West, value, this.East);
            }
        }

        public double East
        {
            get
            {
                if (this.halfWidth != 180.0)
                {
                    return NormalizeLongitude(this.Center.Longitude + this.halfWidth);
                }
                return 180.0;
            }
            set
            {
                this.Init(this.North, this.West, this.South, value);
            }
        }

        public Position Center { get; private set; }

        public double Width
        {
            get
            {
                return this.halfWidth * 2.0;
            }
        }

        public double Height
        {
            get
            {
                return this.halfHeight * 2.0;
            }
        }

        public Position Northwest
        {
            get
            {
                return new Position() { Latitude = this.North, Longitude = this.West };
            }
            set
            {
                if (this.Center == (Position)null)
                {
                    this.Init(value.Latitude, value.Longitude, value.Latitude, value.Longitude);
                }
                else
                {
                    this.Init(value.Latitude, value.Longitude, this.South, this.East);
                }
            }
        }

        public Position Northeast
        {
            get
            {
                return new Position() { Latitude = this.North, Longitude = this.East };
            }
            set
            {
                if (this.Center == (Position)null)
                {
                    this.Init(value.Latitude, value.Longitude, value.Latitude, value.Longitude);
                }
                else
                {
                    this.Init(value.Latitude, this.West, this.South, value.Longitude);
                }
            }
        }

        public Position Southeast
        {
            get
            {
                return new Position() { Latitude = this.South, Longitude = this.East };
            }
            set
            {
                if (this.Center == (Position)null)
                {
                    this.Init(value.Latitude, value.Longitude, value.Latitude, value.Longitude);
                }
                else
                {
                    this.Init(this.North, this.West, value.Latitude, value.Longitude);
                }
            }
        }

        public Position Southwest
        {
            get
            {
                return new Position() { Latitude = this.South, Longitude = this.West };
            }
            set
            {
                if (this.Center == (Position)null)
                {
                    this.Init(value.Latitude, value.Longitude, value.Latitude, value.Longitude);
                }
                else
                {
                    this.Init(this.North, value.Longitude, value.Latitude, this.East);
                }
            }
        }

        public LocationRect()
        {
            this.Center = new Position(0.0, 0.0);
        }

        public LocationRect(Position center, double width, double height)
        {
            this.Center = center;
            this.halfWidth = width / 2.0;
            this.halfHeight = height / 2.0;
        }

        public LocationRect(double north, double west, double south, double east)
            : this()
        {
            this.Init(north, west, south, east);
        }

        public LocationRect(LocationRect rect)
        {
            this.Center = new Position(rect.Center.Latitude, rect.Center.Longitude);
            this.halfHeight = rect.halfHeight;
            this.halfWidth = rect.halfWidth;
        }

        public static LocationRect CreateLocationRect(params Position[] locations)
        {
            return CreateLocationRect((IEnumerable<Position>)locations);
        }

        public static LocationRect CreateLocationRect(IEnumerable<Position> positions)
        {
            if (positions == null)
            {
                return new LocationRect();
            }
            double num1 = -90.0;
            double num2 = 90.0;
            double num3 = 180.0;
            double num4 = -180.0;
            foreach (var position in positions)
            {
                num1 = Math.Max(num1, position.Latitude);
                num2 = Math.Min(num2, position.Latitude);
                num3 = Math.Min(num3, position.Longitude);
                num4 = Math.Max(num4, position.Longitude);
            }
            return new LocationRect(num1, num3, num2, num4);
        }

        private void Init(double north, double west, double south, double east)
        {
            if (west > east)
            {
                east += 360.0;
            }
            this.Center = new Position() { Latitude = (south + north) / 2.0, Longitude = NormalizeLongitude((west + east) / 2.0) };
            this.halfHeight = (north - south) / 2.0;
            this.halfWidth = Math.Abs(east - west) / 2.0;
        }

        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return string.Format(provider, "{0:" + format + "} {1:" + format + "}", new object[2] { (object)this.Northwest, (object)this.Southeast });
        }

        public bool Equals(LocationRect value)
        {
            return this == value;
        }

        public override int GetHashCode()
        {
            return this.Center.GetHashCode() ^ this.halfWidth.GetHashCode() ^ this.halfHeight.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            LocationRect locationRect = obj as LocationRect;
            if (locationRect == null || !(this.Center == locationRect.Center) || this.halfWidth != locationRect.halfWidth)
            {
                return false;
            }
            return this.halfHeight == locationRect.halfHeight;
        }

        public bool Intersects(LocationRect rect)
        {
            double num1 = Math.Abs(this.Center.Latitude - rect.Center.Latitude);
            double num2 = Math.Abs(this.Center.Longitude - rect.Center.Longitude);
            if (num2 > 180.0)
            {
                num2 = 360.0 - num2;
            }
            if (num1 <= this.halfHeight + rect.halfHeight)
            {
                return num2 <= this.halfWidth + rect.halfWidth;
            }
            return false;
        }

        public LocationRect Intersection(LocationRect rect)
        {
            LocationRect locationRect = new LocationRect();
            if (this.Intersects(rect))
            {
                double val1_1 = this.Center.Longitude - this.halfWidth;
                double val2_1 = rect.Center.Longitude - rect.halfWidth;
                double val1_2 = this.Center.Longitude + this.halfWidth;
                double val2_2 = rect.Center.Longitude + rect.halfWidth;
                if (Math.Abs(this.Center.Longitude - rect.Center.Longitude) > 180.0)
                {
                    if (this.Center.Longitude < rect.Center.Longitude)
                    {
                        val1_1 += 360.0;
                        val1_2 += 360.0;
                    }
                    else
                    {
                        val2_1 += 360.0;
                        val2_2 += 360.0;
                    }
                }
                double num1 = Math.Max(val1_1, val2_1);
                double num2 = Math.Min(val1_2, val2_2);
                double num3 = Math.Min(this.North, rect.North);
                double num4 = Math.Max(this.South, rect.South);
                locationRect = new LocationRect(new Position { Latitude = (num3 + num4) / 2.0, Longitude = NormalizeLongitude((num1 + num2) / 2.0) }, num2 - num1, num3 - num4);
            }
            return locationRect;
        }

        public override string ToString()
        {
            return ((IFormattable)this).ToString((string)null, (IFormatProvider)null);
        }

        public string ToString(IFormatProvider provider)
        {
            return ((IFormattable)this).ToString((string)null, provider);
        }

        private static double NormalizeLongitude(double longitude)
        {
            if (longitude < -180.0 || longitude > 180.0)
            {
                return longitude - Math.Floor((longitude + 180.0) / 360.0) * 360.0;
            }
            return longitude;
        }
    }
}