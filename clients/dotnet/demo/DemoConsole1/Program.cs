using Microsoft.Extensions.DependencyInjection;
using Cancelify.Redis;
using Cancelify.Core;

var services = new ServiceCollection();
services.AddRedisCancelify("localhost:6379");


var provider = services.BuildServiceProvider();
var manager = provider.GetRequiredService<IDistributedCancellationToken>();

string jobId = "task-oguz-123";

while (true)
{
    await Task.Delay(1000);

    Console.WriteLine("Type c if you want to cancel...");
    if(Console.ReadLine() == "c") {
        await manager.CancelAsync(jobId);

        break;
    }
}