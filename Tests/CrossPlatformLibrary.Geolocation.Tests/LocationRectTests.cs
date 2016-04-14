using System;
using System.Linq;

using FluentAssertions;

using Xunit;

namespace CrossPlatformLibrary.Geolocation.Tests
{
    public class LocationRectTests
    {
        [Fact]
        public void ShouldContainsZeroLatitudeLongitudeInZeroLocationRect()
        {
            // Arrange
            var locationRect = new LocationRect(0,0,0,0);

            // Act
            var isContained = locationRect.Contains(new Position(0, 0));

            // Assert
            isContained.Should().BeTrue();
        }
    }
}
