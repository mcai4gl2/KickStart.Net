using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KickStart.Net.Extensions;
using NUnit.Framework;

namespace KickStart.Net.Tests.Extensions
{
    [TestFixture]
#if !NET_CORE
    [Timeout(5000)]
#endif
    public class TaskFactoryExtensionsTests
    {
        [Test]
        public async Task test_schedule()
        {
            var tcs = new TaskCompletionSource<bool>();
            await Task.Factory.Schedule(() =>
            {
                tcs.SetResult(true);
            }, 1000, TimeUnits.Milliseconds);
            Assert.IsFalse(tcs.Task.IsCompleted);
            await tcs.Task;
            Assert.IsTrue(tcs.Task.Result);
        }

        [Test]
        public async Task can_cancel_schedule_at_fixed_rate()
        {
            var tokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<bool>();
            var task = await Task.Factory.ScheduleAtFixedRate(() =>
            {
                tcs.SetResult(true);
            }, 1000, 500, TimeUnits.Milliseconds, tokenSource.Token);
            tokenSource.Cancel();
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(tcs.Task.IsCompleted);
        }

        [Test]
        public async Task exception_in_action_would_stop_further_schedule_at_fixed_rate()
        {
            var first = true;
            var tcs = new TaskCompletionSource<bool>();
            var task = await Task.Factory.ScheduleAtFixedRate(() =>
            {
                if (first)
                {
                    first = false;
                    throw new Exception();
                }
                tcs.SetResult(true);
            }, 0, 500, TimeUnits.Milliseconds);
            await 0.5.Seconds();
            Assert.IsTrue(task.IsFaulted);
            Assert.IsFalse(tcs.Task.IsCompleted);
        }

        [Test]
        public async Task can_schedule_at_fixed_rate()
        {
            var results = new List<int>();
            var count = 1;
            var tokenSource = new CancellationTokenSource();
            var task = await Task.Factory.ScheduleAtFixedRate(() =>
            {
                results.Add(count++);
                Thread.Sleep(50);
            }, 1000, 50, TimeUnits.Milliseconds, tokenSource.Token);
            await 2.Seconds();
            tokenSource.Cancel();
            await 1.Seconds();
            Assert.IsTrue(task.IsCanceled || task.IsCompleted);
            Assert.That(results.Count > 0);
            Assert.AreEqual(20, results.Count, 3);
        }

        [Test]
        public async Task can_cancel_schedule_at_fixed_delay()
        {
            var tokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<bool>();
            var task = await Task.Factory.ScheduleAtFixedDelay(() =>
            {
                tcs.SetResult(true);
            }, 1000, 500, TimeUnits.Milliseconds, tokenSource.Token);
            tokenSource.Cancel();
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(tcs.Task.IsCompleted);
        }

        [Test]
        public async Task exception_in_action_would_stop_further_schedule_at_fixed_delay()
        {
            var first = true;
            var tcs = new TaskCompletionSource<bool>();
            var task = await Task.Factory.ScheduleAtFixedDelay(() =>
            {
                if (first)
                {
                    first = false;
                    throw new Exception();
                }
                tcs.SetResult(true);
            }, 0, 500, TimeUnits.Milliseconds);
            await 0.5.Seconds();
            Assert.IsTrue(task.IsFaulted);
            Assert.IsFalse(tcs.Task.IsCompleted);
        }

        [Test]
        public async Task can_schedule_at_fixed_delay()
        {
            var results = new List<int>();
            var count = 1;
            var tokenSource = new CancellationTokenSource();
            var task = await Task.Factory.ScheduleAtFixedDelay(() =>
            {
                results.Add(count++);
                Thread.Sleep(50);
            }, 1000, 50, TimeUnits.Milliseconds, tokenSource.Token);
            await 2.Seconds();
            tokenSource.Cancel();
            await 1.Seconds();
            Assert.IsTrue(task.IsCanceled || task.IsCompleted);
            Assert.That(results.Count > 0);
            Assert.AreEqual(10, results.Count, 3);
        }
    }
}
