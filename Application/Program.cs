using System;
using System.Threading;
using LogComponent;
namespace Application
{
	class Program
	{
		static void Main(string[] args)
		{
			TimeService timeService = new TimeService();
			IAsyncLogger logger = new AsyncLogger(timeService);

			for (int i = 0; i < 15; i++)
			{
				logger.WriteLog("Number with Flush: " + i.ToString());
				Thread.Sleep(50);
			}

			logger.StopWithFlush();

			IAsyncLogger logger2 = new AsyncLogger(timeService);

			for (int i = 50; i > 0; i--)
			{
				logger2.WriteLog("Number with No flush: " + i.ToString());
				Thread.Sleep(20);
			}

			logger2.StopWithoutFlush();

			//Console.ReadLine();
		}
	}
}