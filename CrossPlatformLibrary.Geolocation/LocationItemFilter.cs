using System.Collections.Generic;

namespace CrossPlatformLibrary.Geolocation
{
    public static class LocationItemFilter
    {
        public static IEnumerable<T> IsInBoundary<T>(this IEnumerable<T> items, LocationRect boundary) where T : IClusteredGeoObject
        {
            if (IsCrossingMeridian180(boundary))
            {
                // split into two rectangles and test each
                var eastRect = new LocationRect(boundary.North, 180, boundary.South, boundary.East);
                var westRect = new LocationRect(boundary.North, boundary.West, boundary.South, -180);

                foreach (var item in items)
                {
                    if (eastRect.Contains(item.Location) || westRect.Contains(item.Location))
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                foreach (var item in items)
                {
                    if (boundary.Contains(item.Location))
                    {
                        yield return item;
                    }
                }
            }
        }

        private static bool IsCrossingMeridian180(LocationRect boundary)
        {
            return (boundary.East < boundary.West);
        }

        public static bool Contains(this LocationRect locationRect, Position location)
        {
            if (location == null)
            {
                return false;
            }

            return (locationRect.West <= location.Longitude
                && locationRect.East >= location.Longitude
                && locationRect.South <= location.Latitude
                && locationRect.North >= location.Latitude);
        }
    }
}

