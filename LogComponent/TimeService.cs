using System;
using System.Collections.Generic;
using System.Text;

namespace LogComponent
{
    public class TimeService : ITimeService
    {
        public DateTime GetCurrentTimestamp()
        {
            return DateTime.UtcNow;
        }

        public bool IsNewDate(DateTime previousDate, DateTime currentDate)
        {
            return previousDate.Date != currentDate.Date;
        }
    }
}
