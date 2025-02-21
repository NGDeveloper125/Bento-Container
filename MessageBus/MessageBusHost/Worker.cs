using MessageBusDomain;
using MessageBusDomain.Entities;

namespace MessageBusHost;

public class Worker(IConfiguration configuration,
                    ILogger<Worker> logger,
                    ILogger<MessageBus> messageBusLogger,
                    ILogger<Embuser> embuserLogger,
                    ILogger<Debuser> debuserLogger,
                    ILogger<RecoveryHandler> recoveryHandlerLogger) : BackgroundService
{
    private readonly IConfiguration configuration = configuration;
    private readonly ILogger<Worker> logger = logger;
    private readonly ILogger<MessageBus> messageBusLogger = messageBusLogger;
    private readonly ILogger<Embuser> embuserLogger = embuserLogger;
    private readonly ILogger<Debuser> debuserLogger = debuserLogger;
    private readonly ILogger<RecoveryHandler> recoveryHandlerLogger = recoveryHandlerLogger;
    private MessageBus? messageBus;
    private Task? embuserTask;
    private Task? debuserTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Message Bus starting...");
            messageBus = new MessageBus(messageBusLogger, RecoveryHandler.LoadQueueMessages(recoveryHandlerLogger));

            var embuserInfo = new EmbuserInfo(configuration["Embuser:Address"]!, configuration["Embuser:Port"]!);
            var debuserInfo = new DebuserInfo(configuration["Debuser:Address"]!, configuration["Debuser:Port"]!);

            var embuser = new Embuser(embuserInfo, messageBus, embuserLogger);
            var debuser = new Debuser(debuserInfo, messageBus, debuserLogger);

            logger.LogInformation($"Embuser starting on {embuserInfo.Address.AddressString}:{embuserInfo.Port.PortNumber}");
            embuserTask = Task.Run(() => { embuser.Run(cancellationToken); }, cancellationToken);

            logger.LogInformation($"Debuser starting on {debuserInfo.Address.AddressString}:{debuserInfo.Port.PortNumber}");
            debuserTask = Task.Run(() => { debuser.Run(cancellationToken); }, cancellationToken);

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
        RecoveryHandler.SaveQueueMessages(messageBus!.Queue, recoveryHandlerLogger);
        await Task.WhenAll(embuserTask!, debuserTask!);

        logger.LogInformation("Message Bus stopped.");
        await base.StopAsync(cancellationToken);
    }
}
