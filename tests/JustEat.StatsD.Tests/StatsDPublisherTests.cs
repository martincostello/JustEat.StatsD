using Moq;

namespace JustEat.StatsD;

public static class StatsDPublisherTests
{
    [Fact]
    public static void Decrement_Sends_Multiple_Metrics()
    {
        // Arrange
        var mock = new Mock<IStatsDTransport>();

        var config = new StatsDConfiguration
        {
            Prefix = "red",
        };

        using (var publisher = new StatsDPublisher(config, mock.Object))
        {
            // Act
            publisher.Decrement(10, "black");
            publisher.Decrement(-10, "yellow");
            publisher.Decrement(10, 1, "pink");
            publisher.Decrement(-10, 1, "orange");
            publisher.Decrement(10, 1, new[] { "white", "blue" });
            publisher.Decrement(10, 1, new List<string>() { "green", "red" });
        }

        // Assert
        mock.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Exactly(8));
    }

    [Fact]
    public static void Increment_Sends_Multiple_Metrics()
    {
        // Arrange
        var mock = new Mock<IStatsDTransport>();

        var config = new StatsDConfiguration
        {
            Prefix = "red",
        };

        using (var publisher = new StatsDPublisher(config, mock.Object))
        {
            // Act
            publisher.Increment(10, "black");
            publisher.Increment(-10, "yellow");
            publisher.Increment(10, 1, "pink");
            publisher.Increment(-10, 1, "orange");
            publisher.Increment(10, 1, new[] { "white", "blue" });
            publisher.Increment(10, 1, new List<string>() { "green", "red" });
        }

        // Assert
        mock.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Exactly(8));
    }

    [Fact]
    public static void Metrics_Sent_If_Tags_Are_Null()
    {
        // Arrange
        var mock = new Mock<IStatsDTransport>();

        var config = new StatsDConfiguration
        {
            Prefix = "red",
        };

        using (var publisher = new StatsDPublisher(config, mock.Object))
        {
            // Act
            publisher.Increment(10, 1.0, "black");
            publisher.Gauge(10, "black");
            publisher.Timing(10, 1.0, "black");
        }

        // Assert
        mock.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Exactly(3));
    }

    [Fact]
    public static void Metrics_Not_Sent_If_Array_Is_Null_Or_Empty()
    {
        // Arrange
        var mock = new Mock<IStatsDTransport>();
        var config = new StatsDConfiguration();
        var anyValidTags = new Dictionary<string, string?>
        {
            ["foo"] = "bar",
            ["empty"] = null,
            ["lorem"] = "ipsum",
        };

        using (var publisher = new StatsDPublisher(config, mock.Object))
        {
            // Act
#nullable disable
            publisher.Decrement(-1, 1, null as string[]);
            publisher.Increment(-1, 1, null as string[]);
            publisher.Decrement(1, 1, null as string[]);
            publisher.Increment(1, 1, null as string[]);
            publisher.Decrement(1, 1, Array.Empty<string>());
            publisher.Increment(1, 1, Array.Empty<string>());
            publisher.Decrement(-1, 1, Array.Empty<string>());
            publisher.Increment(-1, 1, Array.Empty<string>());
            publisher.Decrement(-1, 1, new List<string>());
            publisher.Increment(-1, 1, new List<string>());
            publisher.Decrement(1, 1, null as IEnumerable<string>);
            publisher.Increment(1, 1, null as IEnumerable<string>);
            publisher.Decrement(1, 1, new[] { string.Empty });
            publisher.Increment(1, 1, new[] { string.Empty });
            publisher.Decrement(1, 1, new[] { string.Empty }, anyValidTags);
            publisher.Increment(1, 1, new[] { string.Empty }, anyValidTags);
            publisher.Decrement(1, 1, anyValidTags, new[] { string.Empty });
            publisher.Increment(1, 1, anyValidTags, new[] { string.Empty });
            publisher.Decrement(1, 1, anyValidTags, null as string[]);
            publisher.Increment(1, 1, anyValidTags, null as string[]);
#nullable enable
        }

        // Assert
        mock.Verify((p) => p.Send(It.IsAny<ArraySegment<byte>>()), Times.Never());
    }

    [Fact]
    public static void Metrics_Not_Sent_If_No_Metrics()
    {
        // Arrange
        var mock = new Mock<IStatsDTransport>();
        var config = new StatsDConfiguration();

        using (var publisher = new StatsDPublisher(config, mock.Object))
        {
            // Act
            publisher.Decrement(1, 0, new[] { "foo" });
            publisher.Increment(1, 0, new[] { "bar" });
        }

        // Assert
        mock.Verify((p) => p.Send(It.IsAny<ArraySegment<byte>>()), Times.Never());
    }

    [Fact]
    public static void Constructor_Throws_If_Configuration_Is_Null()
    {
        // Arrange
        StatsDConfiguration? configuration = null;
        var transport = Mock.Of<IStatsDTransport>();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(
            "configuration",
            () => new StatsDPublisher(configuration!, transport));
    }

    [Fact]
    public static void Constructor_Throws_If_Transport_Is_Null()
    {
        // Arrange
        var configuration = new StatsDConfiguration();
        IStatsDTransport? transport = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(
            "transport",
            () => new StatsDPublisher(configuration, transport!));
    }

    [Fact]
    public static void Constructor_Does_Not_Throw_If_Tags_Formatter_Is_Null()
    {
        // Arrange
        var configuration = new StatsDConfiguration
        {
            TagsFormatter = null!,
        };
        var transport = Mock.Of<IStatsDTransport>();

        // Act
        using var publisher = new StatsDPublisher(configuration, transport);

        // Assert
        Assert.NotNull(publisher);
    }
}
