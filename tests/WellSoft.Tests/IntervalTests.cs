using WellSoft.Models;
using Xunit;

namespace WellSoft.Tests
{
    public class IntervalTests
    {
        [Theory]
        [InlineData(-1, 10, 0.1, "negative depth")]
        [InlineData(5, 5, 0.5, "depthFrom >= depthTo")]
        [InlineData(0, 10, 1.2, "porosity > 1")]
        [InlineData(0, 10, -0.5, "porosity < 0")]
        public void Validate_ShouldReturnErrors_WhenInvalidData(double from, double to, double porosity, string testCase)
        {
            var interval = new Interval
            {
                DepthFrom = from,
                DepthTo = to,
                Rock = "Sandstone",
                Porosity = porosity,
                LineNumber = 1
            };
            string wellId = "W1";

            var errors = interval.Validate(wellId).ToList();

            Assert.NotEmpty(errors);
            Assert.All(errors, e => Assert.Equal(wellId, e.WellId));
        }

        [Fact]
        public void Validate_ShouldReturnEmpty_WhenValidData()
        {
            var interval = new Interval
            {
                DepthFrom = 10,
                DepthTo = 20,
                Rock = "Limestone",
                Porosity = 0.15,
                LineNumber = 2
            };
            string wellId = "W2";

            var errors = interval.Validate(wellId);

            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenRockIsEmpty()
        {
            var interval = new Interval
            {
                DepthFrom = 0,
                DepthTo = 10,
                Rock = "   ",
                Porosity = 0.1,
                LineNumber = 3
            };
            string wellId = "W3";

            var errors = interval.Validate(wellId).ToList();

            Assert.Single(errors);
            Assert.Contains("Rock не может быть пустым", errors[0].Message);
        }
    }
}
