using System;
using System.Collections.Generic;
using AzureNetQ.SystemMessages;

namespace AzureNetQ.Scheduler.Tests
{
    public class MockScheduleRepository : IScheduleRepository
    {
        public Func<IList<ScheduleMe>> GetPendingDelegate { get; set; } 

        public void Store(ScheduleMe scheduleMe)
        {
            throw new NotImplementedException();
        }

        public IList<ScheduleMe> GetPending()
        {
            return (GetPendingDelegate != null)
                       ? GetPendingDelegate()
                       : null;
        }

        public void Purge()
        {
            throw new NotImplementedException();
        }
    }
}