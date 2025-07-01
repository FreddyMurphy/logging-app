namespace LogComponent
{
    using System;
	using System.IO;
	using System.Text;
	using System.Threading;
	using System.Diagnostics;
    using System.Collections.Concurrent;

    public class AsyncLogger : IAsyncLogger
    {
        private string _basePath = @"./LogTest";
        private ITimeService _timeService;
        private Thread _runThread;
        private ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();
        private DateTime? _curDate;
        private StreamWriter _writer;

        private volatile bool _quitWithFlush = false;
        private volatile bool _exit = false;

        public AsyncLogger(ITimeService timeService)
        {
            _timeService = timeService;
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            _runThread = new Thread(MainLoop);
            _runThread.Start();
        }

        private void MainLoop()
        {
            while (ShouldContinueLogging())
            {
                try
                {
                    if (_lines.TryDequeue(out var logLine))
                    {
                        var tempDate = _timeService.GetCurrentTimestamp();
                        if (!_curDate.HasValue || _timeService.IsNewDate(tempDate, _curDate.Value))
                        {
                            _writer?.Close();
                            var fileName = GetFilePath(_basePath, tempDate);
                            _writer = GetFileLogWriter(fileName);
                            _curDate = tempDate;
                        }

                        _writer.Write(GetLogMessage(logLine));
                    }
                }
                catch (Exception ex)
                {
                    // Swallow error to not crash main application
                    Trace.WriteLine($"Async logger failed with exception: {ex.Message}");
                }
            }
            _writer?.Close();
        }

        private bool ShouldContinueLogging() => !_exit && !(_quitWithFlush && _lines.Count == 0);
        public static string GetFilePath(string basePath, DateTime currentTime) => basePath + "/Log" + currentTime.ToString("yyyyMMdd HHmmss fff") + ".log";
        private static StreamWriter GetFileLogWriter(string fileName)
        {
            var writer = File.AppendText(fileName);
            writer.AutoFlush = true;

            writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);
            return writer;
        }

        private static string GetLogMessage(LogLine logLine)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            stringBuilder.Append("\t");
            stringBuilder.Append(logLine.LineText());
            stringBuilder.Append("\t");

            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        public void StopWithoutFlush()
        {
            _exit = true;
        }

        public void StopWithFlush()
        {
            _quitWithFlush = true;
        }

        public void WriteLog(string s)
        {
            try
            {
                if (!_quitWithFlush)
                {
                    _lines.Enqueue(new LogLine() { Text = s, Timestamp = _timeService.GetCurrentTimestamp() });
                }
            }
            catch (Exception ex)
            {
                // Swallow error to not crash main application
                Trace.WriteLine($"Async logger failed with exception: {ex.Message}");
            }

        }

        public string GetBasePath()
        {
            return _basePath;
        }
    }
}