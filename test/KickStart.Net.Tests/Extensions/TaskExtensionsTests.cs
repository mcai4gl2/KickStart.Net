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
    [Timeout(1000)]
#endif
    public class TaskExtensionsTests
    {
        [Test]
        public async Task test_ToListAsync()
        {
            List<int> result = await Task.Delay(.1.Seconds()).ContinueWith(_ => new[] {1, 2, 3} as IEnumerable<int>).ToListAsync();
            Assert.AreEqual(new List<int> {1,2,3}, result);
        }

        [Test]
        public async Task TimeoutAfter_non_generic()
        {
            var longRunningTask = Task.Delay(5.Seconds());
            var result = longRunningTask.TimeoutAfter(100);
            await result;
        }

        [Test]
        public async Task TimeoutAfter_shall_return_default_value_after_timeout()
        {
            var longRunningTask = Task.Delay(5.Seconds()).ContinueWith(_ => 1);
            var result = await longRunningTask.TimeoutAfter(.5.Seconds());
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task TimeoutAfter_shall_return_result_if_not_timeed_out()
        {
            var longRunningTask = Task.Delay(50).ContinueWith(_ => 1);
            var result = await longRunningTask.TimeoutAfter(100);
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task TimeoutAfter_shall_call_callback_when_timed_out()
        {
            var longRunningTask = Task.Delay(5000).ContinueWith(_ => 1);
            var result = await longRunningTask.TimeoutAfter(100, () => 2);
            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task TimeoutAfter_callback_running_longer_than_main_task_returns_the_actual_result()
        {
            var longRunningTask = Task.Delay(100).ContinueWith(_ => 1);
            var result = await longRunningTask.TimeoutAfter(10, () =>
            {
                Thread.Sleep(200);
                return 2;
            });
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task TimeoutAfter_long_running_callback_but_still_returns_before_the_main_task()
        {
            var longRunningTask = Task.Delay(200).ContinueWith(_ => 1);
            var result = await longRunningTask.TimeoutAfter(10, () =>
            {
                Thread.Sleep(100);
                return 2;
            });
            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task can_ignore_cancellation_exception()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            try
            {
                await Sleep(cts.Token);
                Assert.Fail();
            }
            catch (TaskCanceledException)
            {
                
            }
            await Task.Delay(100, cts.Token).ContinueWhenCancelled();
        }

        private async Task Sleep(CancellationToken token)
        {
            await Task.Delay(100, token);
        }

        [Test]
        public async Task can_negate_a_function()
        {
            Func<Task<bool>> func = () => Task.FromResult(true);
            var negate = func.Not();
            Assert.IsFalse(await negate());
        }
    }
}
