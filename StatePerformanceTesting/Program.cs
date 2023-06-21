using Dapr.Client;
using StackExchange.Redis;
using System.Diagnostics;

namespace StatePerformanceTesting
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            await RunRedisAsync();
            await RunDaprAsync();
        }

        private async static Task RunDaprAsync()
        {
            string DAPR_STORE_NAME = "statestore";

            using var client = new DaprClientBuilder().Build();

            var random = new Random();

            await TestAsync("Dapr read/write test", 1000, async (i) =>
            {
                await client.SaveStateAsync(DAPR_STORE_NAME, $"dapr-key-{i}", $"dapr-value-{i}");
                var result = await client.GetStateAsync<string>(DAPR_STORE_NAME, $"dapr-key-{i}");
            });
        }

        private async static Task RunRedisAsync()
        {
            ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync("redis");
            IDatabase db = redis.GetDatabase();

            await TestAsync("Redist read/write test", 1000, async (i) =>
            {
                await db.StringSetAsync($"direct-key-{i}", $"direct-key-{i}");
                var value = await db.StringGetAsync($"direct-key-{i}");
            });
        }

        private async static Task TestAsync(string actionName, int iterations, Func<int, Task> action)
        {
            Console.WriteLine($"Starting {actionName}.");
            Console.WriteLine($"Running {iterations} times.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < iterations; i++)
            {
                await action(i);
            }

            stopwatch.Stop();

            Console.WriteLine("Completed.");
            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        }
    }
}