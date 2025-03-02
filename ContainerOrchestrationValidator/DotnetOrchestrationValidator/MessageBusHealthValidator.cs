using DotnetMessageBusHub;
using DotnetSharedEntities;
using DotnetSharedEntities.MessageBusModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Text.Json;
using Xunit;

namespace ContainerOrchestrationValidator;

public class TestTask
{
    public TestTask(Guid taskId)
    {
        TaskId = taskId;
    }
    public Guid TaskId { get; set; }
}

public class MessageBusHealthValidator
{
    private string enqueuerUri;
    private string dequeuerUri;

    public MessageBusHealthValidator()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.GetFullPath("../../BentoConfiguration.json"))
            .Build();

        enqueuerUri = ConfigurationHandler.GetEnqueuerUri(configuration);
        dequeuerUri = ConfigurationHandler.GetDequeuerUri(configuration);
    }

    [Fact]
    public async Task MessageBusRunning()
    {
        // This test is to be run when the container is spinned up with the messagebus running inside it.
        TestTask testTask = new TestTask(Guid.NewGuid());
        await MessageBusHub.PushMessageToBus<TestTask>(testTask, "HealthValidationTest", enqueuerUri);

        Thread.Sleep(3000);
        PulledMessage pulledMessage = await MessageBusHub.PullMessageFromBus("HealthValidationTest", dequeuerUri, new CancellationToken());
        Assert.True(pulledMessage.SuccessfullyPulled);

        TestTask? testTaskResponse = JsonSerializer.Deserialize<TestTask>(pulledMessage.Payload);
        Assert.NotNull(testTaskResponse);
        Assert.Equal(testTask.TaskId, testTaskResponse.TaskId);
    }
}