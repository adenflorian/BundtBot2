using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BundtBot;

namespace BundtCommon
{
    public class Wait
    {
        Func<bool> _condition;
        TimeSpan _checkInterval = TimeEx._100ms;
        TimeSpan _timeout = TimeSpan.MaxValue;

        public static async Task AndLogAsync(TimeSpan waitAmount, MyLogger logger)
        {
            logger.LogInfo($"Waiting {waitAmount.TotalSeconds} seconds...");
            await Task.Delay(waitAmount);
        }

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