using LogComponent;
using LogComponent.testing;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using Xunit;

namespace LogComponentTests
{
    public class AsyncLoggerTests
    {
        [Fact]
        public void GIVEN_AsyncLogger_WHEN_Logging_THEN_FileContainsMessage()
        {
            // GIVEN
            var clock = new FakeTimeService();
            var logger = new AsyncLogger(clock);
            var log = "Before midnight";
            var time = new DateTime(2025, 6, 30, 00, 00, 00);

            // WHEN
            clock.dateTimeOverride = time;
            logger.WriteLog(log);
            logger.StopWithFlush();
            Thread.Sleep(10);

            // THEN
            var basePath = logger.GetBasePath();

            var file = AsyncLogger.GetFilePath(basePath, time);
            Assert.True(File.Exists(file));
            Assert.Contains(log, File.ReadAllText(file));
            Assert.Equal(2, File.ReadAllLines(file).Length); // header and log line
            File.Delete(file); // Clean up after test
        }

        [Fact]
        public void GIVEN_AsyncLogger_WHEN_StoppingWithFlush_THEN_FileContainsAllMessage()
        {
            // GIVEN
            var clock = new FakeTimeService();
            var logger = new AsyncLogger(clock);
            var time = new DateTime(2025, 6, 30, 20, 15, 00);
            var numberOfLogs = 1000;

            // WHEN
            clock.dateTimeOverride = time;
            for (int i = 1; i <= numberOfLogs; i++)
            {
                logger.WriteLog("Number with Flush: " + i.ToString());
            }
            logger.StopWithFlush();
            Thread.Sleep(100);

            // THEN
            var basePath = logger.GetBasePath();

            var file = AsyncLogger.GetFilePath(basePath, time);
            Assert.True(File.Exists(file));
            var text = File.ReadAllText(file);
            Assert.Contains(numberOfLogs.ToString(), File.ReadAllText(file));
            Assert.Equal(numberOfLogs + 1, File.ReadAllLines(file).Length); // +1 for the header line
            File.Delete(file); // Clean up after test
        }

        [Fact]
        public void GIVEN_AsyncLogger_WHEN_StoppingWithFlushAndLoggingAfter_THEN_FileContainsMessagesBeforeStopping()
        {
            // GIVEN
            var clock = new FakeTimeService();
            var logger = new AsyncLogger(clock);
            var time = new DateTime(2025, 6, 30, 20, 20, 00);
            var numberOfLogs = 1000;

            // WHEN
            clock.dateTimeOverride = time;
            for (int i = 1; i <= numberOfLogs; i++)
            {
                logger.WriteLog("Number with Flush: " + i.ToString());
            }
            logger.StopWithFlush();

            var notIncludedLog = "This log should not be included";
            logger.WriteLog(notIncludedLog);
            Thread.Sleep(100);

            // THEN
            var basePath = logger.GetBasePath();

            var file = AsyncLogger.GetFilePath(basePath, time);
            Assert.True(File.Exists(file));
            var fileText = File.ReadAllText(file);
            Assert.Contains(numberOfLogs.ToString(), fileText);
            Assert.Equal(numberOfLogs + 1, File.ReadAllLines(file).Length); // +1 for the header line
            Assert.DoesNotContain(notIncludedLog, File.ReadAllText(file)); 
            File.Delete(file); // Clean up after test
        }

        [Fact]
        public void GIVEN_AsyncLogger_WHEN_StoppingWithoutFlush_THEN_FileDoesntContainAllMessages()
        {
            // GIVEN
            var clock = new FakeTimeService();
            var logger = new AsyncLogger(clock);
            var time = new DateTime(2025, 6, 30, 20, 15, 00);
            var numberOfLogs = 100000;

            // WHEN
            clock.dateTimeOverride = time;
            for (int i = 1; i <= numberOfLogs; i++)
            {
                logger.WriteLog("Number with Flush: " + i.ToString());
            }
            logger.StopWithoutFlush();
            Thread.Sleep(100);

            // THEN
            var basePath = logger.GetBasePath();

            var file = AsyncLogger.GetFilePath(basePath, time);
            Assert.True(File.Exists(file));
            var text = File.ReadAllText(file);
            Assert.DoesNotContain(numberOfLogs.ToString(), File.ReadAllText(file));
            File.Delete(file); // Clean up after test
        }

        [Fact]
        public void GIVEN_AsyncLogger_WHEN_LoggingOnTwoDifferentDates_THEN_TwoFilesExists()
        {
            // GIVEN
            var times = new[]
            {
                new DateTime(2025, 6, 30, 23, 59, 50),
                new DateTime(2025, 7, 1, 0, 0, 5)
            };
            var clock = new FakeTimeService();
            var logger = new AsyncLogger(clock);
            var log1 = "Before midnight";
            var log2 = "After midnight";

            // WHEN
            clock.dateTimeOverride = times[0];
            logger.WriteLog(log1);
            Thread.Sleep(10);

            clock.dateTimeOverride = times[1];
            logger.WriteLog(log2);
            logger.StopWithFlush();
            Thread.Sleep(10);

            // THEN
            var basePath = logger.GetBasePath();
            var file1 = AsyncLogger.GetFilePath(basePath, times[0]);
            Assert.True(File.Exists(file1));
            Assert.Contains(log1, File.ReadAllText(file1));
            var file2 = AsyncLogger.GetFilePath(basePath, times[1]);
            Assert.True(File.Exists(file2));
            Assert.Contains(log2, File.ReadAllText(file2));
            File.Delete(file1); // Clean up after test
            File.Delete(file2); // Clean up after test
        }

        [Fact]
        public void GIVEN_AsyncLogger_WHEN_WriteLogException_THEN_MessageIsWrittenToTrace()
        {
            // GIVEN
            var timeServiceMock = new Mock<ITimeService>();
            timeServiceMock.Setup(ts => ts.GetCurrentTimestamp()).Throws(new InvalidOperationException("Test exception"));

            var logger = new AsyncLogger(timeServiceMock.Object);
            using var sw = new StringWriter();
            Trace.Listeners.Add(new TextWriterTraceListener(sw));

            // WHEN
            logger.WriteLog("Test log");
            logger.StopWithFlush();
            Thread.Sleep(10);

            // THEN
            var traceOutput = sw.ToString();
            Assert.Contains("Async logger failed with exception", traceOutput);
        }

        [Fact]
        public void GIVEN_AsyncLogger_WHEN_MainLoopException_THEN_MessageIsWrittenToTrace()
        {
            // GIVEN
            var timeServiceMock = new Mock<ITimeService>();
            timeServiceMock.Setup(ts => ts.IsNewDate(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Throws(new InvalidOperationException("Test exception"));

            var logger = new AsyncLogger(timeServiceMock.Object);
            using var sw = new StringWriter();
            Trace.Listeners.Add(new TextWriterTraceListener(sw));

            // WHEN
            logger.WriteLog("Test log");
            logger.WriteLog("Another log");
            logger.StopWithFlush();
            Thread.Sleep(10); 

            // THEN
            var traceOutput = sw.ToString();
            Assert.Contains("Async logger failed with exception", traceOutput);
        }
    }
}