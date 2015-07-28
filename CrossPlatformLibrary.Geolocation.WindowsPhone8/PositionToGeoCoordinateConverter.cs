using System;
using System.Device.Location;
using System.Globalization;
using System.Windows.Data;

namespace CrossPlatformLibrary.Geolocation
{
    public class PositionToGeoCoordinateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = value as Position;
            if (position == null)
            {
                return GeoCoordinate.Unknown;
            }

            return position.ToGeoCoordinate();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var geoCoordinate = value as GeoCoordinate;
            if (geoCoordinate == null)
            {
                return Position.Unknown;
            }

            return geoCoordinate.ToPosition();
        }
    }
}