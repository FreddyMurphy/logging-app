namespace LogTest
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading;

	public class AsyncLogInterface : LogInterface
	{
		private string _basePath = @"./LogTest";
		private Thread _runThread;
		private Queue<LogLine> _lines = new Queue<LogLine>();
		DateTime _curDate = DateTime.Now;
		private StreamWriter _writer;

		private bool _quitWithFlush = false;
		private bool _exit = false;

		public AsyncLogInterface()
		{
			if (!Directory.Exists(this._basePath))
				Directory.CreateDirectory(this._basePath);

			this._writer = File.AppendText(this._basePath + "/Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");
			this._writer.AutoFlush = true;

			this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);
			StringBuilder stringBuilder = new StringBuilder();

			this._runThread = new Thread(this.MainLoop);
			this._runThread.Start();
		}

		private void MainLoop()
		{
			while (!this._exit)
			{
                if (this._quitWithFlush && this._lines.Count == 0)
                {
					break;
                }

				if (this._lines.Count == 0)
				{
					continue;
				}

				var logLine = this._lines.Dequeue();

				StringBuilder stringBuilder = new StringBuilder();

				if ((DateTime.Now - _curDate).Days != 0)
				{
					_curDate = DateTime.Now;

					this._writer = File.AppendText(@"./LogTest/Log" + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");
					this._writer.AutoFlush = true;

					this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);
				}

				stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
				stringBuilder.Append("\t");
				stringBuilder.Append(logLine.LineText());
				stringBuilder.Append("\t");

				stringBuilder.Append(Environment.NewLine);

				this._writer.Write(stringBuilder.ToString());

				Thread.Sleep(50);
			}
		}

		private bool ContinueLogging() => !this._exit && (!this._quitWithFlush && this._lines.Count == 0);

		public void StopWithoutFlush()
		{
			this._exit = true;
		}

		public void StopWithFlush()
		{
			this._quitWithFlush = true;
		}

		public void WriteLog(string s)
		{
			this._lines.Enqueue(new LogLine() {Text = s, Timestamp = DateTime.Now});
		}
	}
}