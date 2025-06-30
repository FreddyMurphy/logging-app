using System;
using System.Collections.Generic;
using System.Text;

namespace LogComponent
{
    public interface ITimeService
    {
        DateTime GetCurrentTimestamp();
        bool IsNewDate(DateTime previousDate, DateTime currentDate);
    }
}
