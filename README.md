# DistributedCancellationToken

A lightweight, pluggable distributed cancellation token system for .NET using Redis or RabbitMQ.

## ✨ Features

- Redis and RabbitMQ implementations
- Easy DI integration with IServiceCollection
- Simple, production-safe API
- Supports cancellation from multiple distributed sources
- In-memory support for testing (optional)

---

## 📦 Installation

> Add project reference or future NuGet package:

bash
dotnet add package DistributedCancellationToken


---

## 🚀 Usage

### Register Redis implementation

csharp
builder.Services.AddRedisDistCancellationToken("localhost:6379");


### Register RabbitMQ implementation

csharp
builder.Services.AddRabbitMqDistCancellationToken("amqp://guest:guest@localhost:5672");


### Inject and use

csharp
public class JobService
{
    private readonly IDistributedCancellationToken _dct;

    public JobService(IDistributedCancellationToken dct)
    {
        _dct = dct;
    }

    public async Task RunAsync(string jobId)
    {
        var token = _dct.GetToken(jobId);
        await Task.Delay(10000, token);
    }

    public Task CancelAsync(string jobId)
    {
        return _dct.CancelAsync(jobId);
    }
}


---

## 🔄 Interfaces

csharp
public interface IDistributedCancellationToken
{
    CancellationToken GetToken(string id);
    Task CancelAsync(string id);
}


---

## ✅ Recommendations

- Only register one implementation (Redis or RabbitMQ)
- IDistributedCancellationToken is a singleton in DI
- For local tests, consider InMemoryDistributedCancellationToken

---

## 📄 License

MIT

---

## 🙌 Contributions

PRs and issues welcome. Focus is simplicity and reliability.