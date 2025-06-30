using System;
using System.Collections.Generic;
using System.Text;

namespace LogComponent.testing
{
    public class FakeTimeService : ITimeService
    {
        public DateTime? dateTimeOverride = null;
        public DateTime GetCurrentTimestamp()
        {
            return dateTimeOverride ?? DateTime.UtcNow;
        }

        public bool IsNewDate(DateTime previousDate, DateTime currentDate)
        {
            return previousDate.Date != currentDate.Date;
        }
    }
}
