using Microsoft.Extensions.DependencyInjection;
using Cancelify.Redis;
using Cancelify.Core;

var services = new ServiceCollection();
services.AddRedisCancelify("localhost:6379");

var provider = services.BuildServiceProvider();
var manager = provider.GetRequiredService<IDistributedCancellationToken>();

string jobId = "task-oguz-123";
var token = manager.GetToken(jobId);

_ = Task.Run(async () => {
	try
	{
		Console.WriteLine("Waiting...");
		await Task.Delay(TimeSpan.FromMinutes(5), token);
	}
	catch (Exception)
	{
        Console.WriteLine("Canceled");
    }
});

Console.ReadLine();

