using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossPlatformLibrary.Geolocation
{
    public static class PositionExtensions
    {
        public static Position GetCentrePoint(this IEnumerable<Position> coordinates)
        {
            if (coordinates == null || !coordinates.Any())
            {
                return Position.Unknown;
            }

            double xSum = 0;
            double ySum = 0;
            double zSum = 0;

            int total = 0;
            foreach (var i in coordinates.Where(x => !x.IsUnknown))
            {
                double lat = i.Latitude * Math.PI / 180;
                double lon = i.Longitude * Math.PI / 180;

                double x = Math.Cos(lat) * Math.Cos(lon);
                double y = Math.Cos(lat) * Math.Sin(lon);
                double z = Math.Sin(lat);

                xSum += x;
                ySum += y;
                zSum += z;

                total++;
            }

            xSum = xSum / total;
            ySum = ySum / total;
            zSum = zSum / total;

            double Lon = Math.Atan2(ySum, xSum);
            double Hyp = Math.Sqrt(xSum * xSum + ySum * ySum);
            double Lat = Math.Atan2(zSum, Hyp);

            return new Position(Lat * 180 / Math.PI, Lon * 180 / Math.PI);
        }

        public static Position GetFarthestCoordinate(this IEnumerable<Position> coordinates, Position centerCoordinate)
        {
            Position farthestCoordinate = null;

            double farthestDistance = 0;
            foreach (var coordinate in coordinates)
            {
                var distance = coordinate.GetDistanceTo(centerCoordinate);
                if (distance > farthestDistance)
                {
                    farthestCoordinate = coordinate;
                    farthestDistance = distance;
                }
            }

            return farthestCoordinate;
        }

        /// <summary>
        ///     Gets the longest radius from center.
        /// </summary>
        /// <param name="coordinates">The coordinates.</param>
        /// <param name="centerCoordinate">The center coordinate.</param>
        /// <param name="zoomLevel">The zoom level.</param>
        /// <returns>System.Double.</returns>
        public static double GetLongestRadiusFromCenter(this IEnumerable<Position> coordinates, Position centerCoordinate, double zoomLevel)
        {
            var farthestCoordinate = coordinates.GetFarthestCoordinate(centerCoordinate);
            if (farthestCoordinate == null)
            {
                return 0;
            }

            var distanceInMeters = centerCoordinate.GetDistanceTo(farthestCoordinate);
            return MetersToPixels(distanceInMeters, centerCoordinate.Latitude, zoomLevel);
        }

        public static double MetersToPixels(double meters, double latitude, double zoomLevel)
        {
            // The ground resolution (in meters per pixel) varies depending on the level of detail
            // and the latitude at which it’s measured. It can be calculated as follows:
            double metersPerPixels = meters / ((Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * 6378137) / (256 * Math.Pow(2, zoomLevel)));
            return metersPerPixels;
        }

        //TODO GATH: This is the official way to translate meters into pixels (BUT IT DOESNT WORK)
        ////public static double MetersToPixels(double meters, double latitude, double zoomLevel)
        ////{
        ////    var pixels = meters / (156543.04 * Math.Cos(latitude) / Math.Pow(2, zoomLevel));
        ////    return Math.Abs(pixels);
        ////}

        /// <summary>
        ///     Gets the accuracy radius in pixels.
        ///     http://developer.nokia.com/resources/library/Lumia/change-history/archived-content/maps-and-navigation/guide-to-the-wp8-maps-api.html
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <param name="zoomLevel">The zoom level.</param>
        public static double GetAccuracyRadius(this Position coordinate, double zoomLevel)
        {
            var radius = MetersToPixels(coordinate.Accuracy, coordinate.Latitude, zoomLevel);
            return radius;
        }

        private const double LatitudeOffset = -0.000025;
        private const double LongitudeOffset = -0.00005;

        /// <summary>
        ///     Add some meters of offset to the given Position.
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <returns>The new Position.</returns>
        public static Position WithRandomOffset(this Position coordinate)
        {
            return new Position(coordinate.Latitude + LatitudeOffset, coordinate.Longitude + LongitudeOffset);
        }

        // TODO GATH: MOVE THIS METHOD TO GEOPROVIDER

        /// <summary>
        /// Searches the GPS coordinates by given address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>Task&lt;Position&gt;.</returns>
        ////public static async Task<Position> SearchPositions(string address)
        ////{
        ////    var taskCompletionSource = new TaskCompletionSource<Position>();
        ////    try
        ////    {
        ////        var query = new GeocodeQuery
        ////        {
        ////            SearchTerm = address,
        ////            Position = new Position(0, 0)
        ////        };

        ////        EventHandler<QueryCompletedEventArgs<IList<MapLocation>>> queryQueryCompleted = null;
        ////        queryQueryCompleted += (s, a) =>
        ////        {
        ////            query.QueryCompleted -= queryQueryCompleted;
        ////            var locations = a.Result
        ////                .Select(location => location.Position)
        ////                .OrderBy(x => x.HorizontalAccuracy)
        ////                .ThenBy(x => x.VerticalAccuracy)
        ////                .FirstOrDefault();

        ////            taskCompletionSource.SetResult(locations);
        ////        };

        ////        query.QueryCompleted += queryQueryCompleted;
        ////        query.QueryAsync();
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Debug.WriteLine(ex.Message);
        ////    }
        ////    return await taskCompletionSource.Task;
        ////}
    }
}