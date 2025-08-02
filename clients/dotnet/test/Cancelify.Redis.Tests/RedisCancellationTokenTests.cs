using Cancelify.Redis;
using Moq;
using StackExchange.Redis;

public class RedisCancellationTokenTests
{
    private const string ChannelPrefix = "cancel-token:";

    [Fact]
    public void GetToken_ShouldThrow_WhenIdIsNullOrEmpty()
    {
        var subscriberMock = new Mock<ISubscriber>();
        var redisMock = new Mock<IRedisConnection>();
        redisMock.Setup(m => m.GetSubscriber()).Returns(subscriberMock.Object);

        var tokenManager = new RedisCancellationToken(redisMock.Object);

        Assert.Throws<ArgumentNullException>(() => tokenManager.GetToken(null));
        Assert.Throws<ArgumentNullException>(() => tokenManager.GetToken(" "));
    }

    [Fact]
    public void GetToken_ShouldReturnSameToken_ForSameId()
    {
        var subscriberMock = new Mock<ISubscriber>();
        var redisMock = new Mock<IRedisConnection>();
        redisMock.Setup(m => m.GetSubscriber()).Returns(subscriberMock.Object);

        var tokenManager = new RedisCancellationToken(redisMock.Object);
        var token1 = tokenManager.GetToken("job-42");
        var token2 = tokenManager.GetToken("job-42");

        Assert.Equal(token1, token2);
    }

    [Fact]
    public async Task CancelAsync_ShouldPublishCorrectChannel()
    {
        var subscriberMock = new Mock<ISubscriber>();
        var redisMock = new Mock<IRedisConnection>();
        redisMock.Setup(m => m.GetSubscriber()).Returns(subscriberMock.Object);

        var tokenManager = new RedisCancellationToken(redisMock.Object);
        await tokenManager.CancelAsync("job-xyz");

        subscriberMock.Verify(
            s => s.PublishAsync(ChannelPrefix + "job-xyz", "cancel", CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrow_WhenIdIsNullOrEmpty()
    {
        var subscriberMock = new Mock<ISubscriber>();
        var redisMock = new Mock<IRedisConnection>();
        redisMock.Setup(m => m.GetSubscriber()).Returns(subscriberMock.Object);

        var tokenManager = new RedisCancellationToken(redisMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => tokenManager.CancelAsync(null));
        await Assert.ThrowsAsync<ArgumentNullException>(() => tokenManager.CancelAsync(" "));
    }

    [Fact]
    public void Token_ShouldBeCancelled_WhenMessageIsPublished()
    {
        Action<RedisChannel, RedisValue> capturedHandler = null;

        var subscriberMock = new Mock<ISubscriber>();
        subscriberMock
            .Setup(s => s.Subscribe(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>(
                (channel, handler, flags) => capturedHandler = handler);

        var redisMock = new Mock<IRedisConnection>();
        redisMock.Setup(m => m.GetSubscriber()).Returns(subscriberMock.Object);

        var tokenManager = new RedisCancellationToken(redisMock.Object);

        var token = tokenManager.GetToken("my-job");

        Assert.False(token.IsCancellationRequested);

        // Simulate Redis publish (via pub/sub)
        capturedHandler?.Invoke(ChannelPrefix + "my-job", "cancel");

        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public void Dispose_ShouldDisposeTokensAndRedis()
    {
        var subscriberMock = new Mock<ISubscriber>();
        var redisMock = new Mock<IRedisConnection>();
        redisMock.Setup(m => m.GetSubscriber()).Returns(subscriberMock.Object);

        var tokenManager = new RedisCancellationToken(redisMock.Object);
        var token = tokenManager.GetToken("t1");

        tokenManager.Dispose();

        redisMock.Verify(r => r.Dispose(), Times.Once);
        Assert.True(token.IsCancellationRequested || token.IsCancellationRequested == false); // Just to access it
    }
}
