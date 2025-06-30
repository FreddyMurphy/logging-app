using System;
using Xunit;
using LogComponent;
using LogComponent.testing;
namespace LogComponentTest
{
    public class UnitTest1
    {
        [Fact]
        public void Logger_CreatesNewFile_WhenMidnightIsCrossed()
        {
            // Arrange
            var times = new[]
            {
                new DateTime(2025, 6, 30, 23, 59, 50),
                new DateTime(2025, 7, 1, 0, 0, 5)
            };
            var timeService = new FakeTimeService();
            var logger = new AsyncLogger(timeService);

            // Act
            logger.Log("Before midnight");
            logger.Log("After midnight");

            // Assert
            Assert.IsTrue(File.Exists("log_2025-06-30.txt"));
            Assert.IsTrue(File.Exists("log_2025-07-01.txt"));
        }
    }
}
