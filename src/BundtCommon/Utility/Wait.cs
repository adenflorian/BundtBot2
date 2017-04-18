using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BundtCommon
{
    public class Wait
    {
        Func<bool> _condition;
        TimeSpan _checkInterval = TimeEx._100ms;
        TimeSpan _timeout = TimeEx._5seconds;

        /// <summary>
        /// Will wait until the condition returns true, or the timeout is reached
        /// </summary>
        public static Wait Until(Func<bool> condition)
        {
            return new Wait(condition);
        }

        Wait(Func<bool> condition)
        {
            _condition = condition;
        }

        public Wait CheckingEvery(TimeSpan checkInterval)
        {
            _checkInterval = checkInterval;
            return this;
        }

        public Wait For(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        public async Task StartAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (_condition.Invoke() == false)
            {
                await Task.Delay(_checkInterval);
                if (stopwatch.Elapsed > _timeout)
                {
                    throw new TimeoutException();
                }
            }
        }
    }
}