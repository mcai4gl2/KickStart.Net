using System;
using System.Threading;
using System.Threading.Tasks;
using KickStart.Net.Metrics;

namespace KickStart.Net.Extensions
{
    public static class TaskFactoryExtensions
    {
        /// <summary>
        /// Executes a one-shot action after a given delay.
        /// </summary>
        public static Task Schedule(this TaskFactory factory, Action action, long delay, ITimeUnit timeUnit)
        {
            return Task.Delay(timeUnit.ToTimeSpan(delay)).ContinueWith(_ => factory.StartNew(action));
        }

        /// <summary>
        /// Executes a periodic action firstly after the given initial delay and subsequently with the given period.
        /// The execution will commence after initialDelay and then initialDelay + period, then initialDelay + 2 * period, and so on.
        /// If any execution of the task encounters an exception, subsequent executions are suppressed.
        /// </summary>
        public static Task<Task> ScheduleAtFixedRate(this TaskFactory factory, Action action, long initialDelay, long period,
            ITimeUnit timeUnit)
        {
            var source = new CancellationTokenSource();
            return ScheduleAtFixedRate(factory, action, initialDelay, period, timeUnit, Clocks.Default, source.Token);
        }

        /// <summary>
        /// Executes a periodic action firstly after the given initial delay and subsequently with the given period.
        /// The execution will commence after initialDelay and then initialDelay + period, then initialDelay + 2 * period, and so on.
        /// If any execution of the task encounters an exception, subsequent executions are suppressed. 
        /// The exeuction can otherwise be cancelled via cancellation token.
        /// </summary>
        public static Task<Task> ScheduleAtFixedRate(this TaskFactory factory, Action action, long initialDelay, long period,
            ITimeUnit timeUnit, CancellationToken token)
        {
            return ScheduleAtFixedRate(factory, action, initialDelay, period, timeUnit, Clocks.Default, token);
        }

        /// <summary>
        /// Executes a periodic action firstly after the given initial delay and subsequently with the given period.
        /// The execution will commence after initialDelay and then initialDelay + period, then initialDelay + 2 * period, and so on.
        /// If any execution of the task encounters an exception, subsequent executions are suppressed. 
        /// The exeuction can otherwise be cancelled via cancellation token.
        /// </summary>
        public static Task<Task> ScheduleAtFixedRate(this TaskFactory factory, Action action, long initialDelay, long period,
            ITimeUnit timeUnit, IClock clock, CancellationToken token)
        {
            return factory.StartNew(async () =>
            {
                var nextRunTime = clock.Tick + timeUnit.ToTicks(initialDelay);
                do
                {
                    var now = clock.Tick;
                    if (now < nextRunTime)
                        await Task.Delay(timeUnit.ToTimeSpan(TimeUnits.Ticks.ToMillis(nextRunTime - now)), token);
                    if (!token.IsCancellationRequested)
                        action();
                    nextRunTime += timeUnit.ToTicks(period);
                } while (!token.IsCancellationRequested);
            }, token);
        }

        /// <summary>
        /// Executes a periodic action firstly after the given initial delay and subsequently with the given delay, 
        /// between the termination of one execution and the commencement of the next.
        /// If any execution of the task encounters an exception, subsequent executions are suppressed. 
        /// </summary>
        public static Task<Task> ScheduleAtFixedDelay(this TaskFactory factory, Action action, long initialDelay,
            long delay, ITimeUnit timeUnit)
        {
            var source = new CancellationTokenSource();
            return ScheduleAtFixedDelay(factory, action, initialDelay, delay, timeUnit, source.Token);
        }

        /// <summary>
        /// Executes a periodic action firstly after the given initial delay and subsequently with the given delay, 
        /// between the termination of one execution and the commencement of the next.
        /// If any execution of the task encounters an exception, subsequent executions are suppressed. 
        /// The exeuction can otherwise be cancelled via cancellation token.
        /// </summary>
        public static Task<Task> ScheduleAtFixedDelay(this TaskFactory factory, Action action, long initialDelay, long delay,
            ITimeUnit timeUnit, CancellationToken token)
        {
            return factory.StartNew(async () =>
            {
                await Task.Delay(timeUnit.ToTimeSpan(initialDelay), token);
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(timeUnit.ToTimeSpan(delay), token);
                } 
            }, token);
        }
    }
}
