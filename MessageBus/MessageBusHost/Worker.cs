using DotnetSharedEntities;
using DotnetSharedEntities.MessageBusModels;
using MessageBusDomain;
using MessageBusDomain.Entities;

namespace MessageBusHost;

public class Worker(IConfiguration configuration, ILogger logger) : BackgroundService
{
    private readonly IConfiguration configuration = configuration;
    private MessageBus? messageBus;
    private Task? enqueuerTask;
    private Task? dequeuerTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Message Bus starting...");
            messageBus = new MessageBus(logger, RecoveryHandler.LoadQueueMessages(logger));

            (EnqueuerInfo enqueuerInfo, DequeuerInfo dequeuerInfo) = GetMessageBusInfo(configuration);

            var enqueuer = new Enqueuer(enqueuerInfo, messageBus, logger);
            var dequeuer = new Dequeuer(dequeuerInfo, messageBus, logger);

            logger.LogInformation($"enqueuer starting on {enqueuerInfo.Address.AddressString}:{enqueuerInfo.Port.PortNumber}");
            enqueuerTask = Task.Run(() => { enqueuer.Run(cancellationToken); }, cancellationToken);

            logger.LogInformation($"dequeuer starting on {dequeuerInfo.Address.AddressString}:{dequeuerInfo.Port.PortNumber}");
            dequeuerTask = Task.Run(() => { dequeuer.Run(cancellationToken); }, cancellationToken);

            await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError($"MessageBus crashed: {ex.Message}");
            throw;
        }
    }

    private static (EnqueuerInfo enqueuerInfo, DequeuerInfo dequeuerInfo) GetMessageBusInfo(IConfiguration configuration)
    {
        string? enqueuerAddress = configuration["MessageBus:Enqueuer:Address"]
                                    ?? throw new ArgumentNullException("Failed to find enqueuer address for message bus");
        string? enqueuerPort = configuration["MessageBus:Enqueuer:Port"]
                                    ?? throw new ArgumentNullException("Failed to find enqueuer port for message bus");
        string? dequeuerAddress = configuration["MessageBus:Dequeuer:Address"]
                                    ?? throw new ArgumentNullException("Failed to find dequeuer address for message bus");
        string? dequeuerPort = configuration["MessageBus:Dequeuer:Port"]
                                    ?? throw new ArgumentNullException("Failed to find dequeuer port for message bus");

        return (new EnqueuerInfo(enqueuerAddress!, enqueuerPort!),
                new DequeuerInfo(dequeuerAddress!, dequeuerPort!));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Message Bus Running");
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1, cancellationToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Message Bus stopping...");
        RecoveryHandler.SaveQueueMessages(messageBus!.Queue, logger);
        await Task.WhenAll(enqueuerTask!, dequeuerTask!);

        logger.LogInformation("Message Bus stopped.");
        await base.StopAsync(cancellationToken);
    }
}
