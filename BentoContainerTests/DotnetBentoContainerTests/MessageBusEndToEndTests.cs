using Xunit;
using FluentAssertions;

namespace DotnetBentoContainerTests;

public class MessageBusEndToEndTests
{
    // this tests are to be run from within a container that runs the message-bus, 
    // better automatically by the dockerfile when spinning up the container. 
    [Fact]
    public void Test1()
    {
        true.Should().BeTrue();
    }
}