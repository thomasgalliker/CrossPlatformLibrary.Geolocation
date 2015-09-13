using System;

using FluentAssertions;

using Xunit;

namespace CrossPlatformLibrary.Geolocation.Tests
{
    public class PositionTests
    {
        [Fact]
        public void ShouldBeUnknownPositionWithDefaultConstructor()
        {
            // Act
            var position = new Position();

            // Assert
            position.IsUnknown.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotBeUnknownPositionWithConcreteLongitudeLatitude()
        {
            // Arrange
            double latitude = 1.0;
            double longitude = 2.0;

            // Act
            var position = new Position(latitude, longitude);

            // Assert
            position.IsUnknown.Should().BeFalse();
        }


        [Fact]
        public void ShouldCopyPositionWithConstructor()
        {
            // Arrange
            double accuracy = 1d;
            double altitude = 2d;
            double altitudeAccuracy = 3d;
            double heading = 4d;
            double latitude = 5d;
            double longitude = 6d;
            double speed = 7d;
            var timestamp = DateTimeOffset.MaxValue;

            var position1 = new Position
            {
                Accuracy = accuracy,
                Altitude = altitude,
                AltitudeAccuracy = altitudeAccuracy,
                Heading = heading,
                Latitude = latitude,
                Longitude = longitude,
                Speed = speed,
                Timestamp = timestamp
            };

            // Act
            var position2 = new Position(position1);

            // Assert
            position2.Accuracy.Should().Be(accuracy);
            position2.Altitude.Should().Be(altitude);
            position2.AltitudeAccuracy.Should().Be(altitudeAccuracy);
            position2.Heading.Should().Be(heading);
            position2.Latitude.Should().Be(latitude);
            position2.Longitude.Should().Be(longitude);
            position2.Speed.Should().Be(speed);
            position2.Timestamp.Should().Be(timestamp);
        }

        #region GetDistanceTo Tests
        [Fact]
        public void ShouldGetDistanceToInKilometers()
        {
            // Arrange
            var position1 = new Position(47.090548d, 8.055840999999986d);     // Geiss, Luzern, Schweiz
            var position2 = new Position(47.181225d, 8.459208900000021d);     // Cham, Zug, Schweiz

            // Act
            var distanceTo = position1.GetDistanceTo(position2, DistanceUnit.Kilometers);

            // Assert
            distanceTo.Should().BeInRange(32.13d, 32.14d);
        }

        [Fact]
        public void ShouldGetDistanceToInMiles()
        {
            // Arrange
            var position1 = new Position(47.090548d, 8.055840999999986d);     // Geiss, Luzern, Schweiz
            var position2 = new Position(47.181225d, 8.459208900000021d);     // Cham, Zug, Schweiz

            // Act
            var distanceTo = position1.GetDistanceTo(position2, DistanceUnit.Miles);

            // Assert
            distanceTo.Should().BeInRange(19.96d, 19.97d);
        }

        [Fact]
        public void ShouldGetDistanceToInNauticalMiles()
        {
            // Arrange
            var position1 = new Position(47.090548d, 8.055840999999986d);     // Geiss, Luzern, Schweiz
            var position2 = new Position(47.181225d, 8.459208900000021d);     // Cham, Zug, Schweiz

            // Act
            var distanceTo = position1.GetDistanceTo(position2, DistanceUnit.NauticalMiles);

            // Assert
            distanceTo.Should().BeInRange(17.33d, 17.34d);
        }
        #endregion
    }
}
