using MessageBusDomain;
using MessageBusDomain.Entities;

namespace MessageBusHost;

public class Worker(IConfiguration configuration, LoggerManager loggerManager) : BackgroundService
{
    private readonly IConfiguration configuration = configuration;
    private readonly ILogger<Worker> logger = loggerManager.WrokerLogger;
    private MessageBus? messageBus;
    private Task? enqueuerTask;
    private Task? dequeuerTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Message Bus starting...");
            messageBus = new MessageBus(loggerManager.MessageBusLogger, RecoveryHandler.LoadQueueMessages(loggerManager.RecoveryHandlerLogger));

            var enqueuerInfo = new EnqueuerInfo(configuration["enqueuer:Address"]!, configuration["enqueuer:Port"]!);
            var dequeuerInfo = new DequeuerInfo(configuration["dequeuer:Address"]!, configuration["dequeuer:Port"]!);

            var enqueuer = new Enqueuer(enqueuerInfo, messageBus, loggerManager.EnqueuerLogger);
            var dequeuer = new Dequeuer(dequeuerInfo, messageBus, loggerManager.DequeuerLogger);

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
        RecoveryHandler.SaveQueueMessages(messageBus!.Queue, loggerManager.RecoveryHandlerLogger);
        await Task.WhenAll(enqueuerTask!, dequeuerTask!);

        logger.LogInformation("Message Bus stopped.");
        await base.StopAsync(cancellationToken);
    }
}
