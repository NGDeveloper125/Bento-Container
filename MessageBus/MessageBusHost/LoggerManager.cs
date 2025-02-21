
using MessageBusDomain;

namespace MessageBusHost;

public class LoggerManager(ILogger<Worker> logger,
                    ILogger<MessageBus> messageBusLogger,
                    ILogger<Enqueuer> enqueuerLogger,
                    ILogger<Dequeuer> dequeuerLogger,
                    ILogger<RecoveryHandler> recoveryHandlerLogger)
{
    public readonly ILogger<Worker> WrokerLogger = logger;
    public readonly ILogger<MessageBus> MessageBusLogger = messageBusLogger;
    public readonly ILogger<Enqueuer> EnqueuerLogger = enqueuerLogger;
    public readonly ILogger<Dequeuer> DequeuerLogger = dequeuerLogger;
    public readonly ILogger<RecoveryHandler> RecoveryHandlerLogger = recoveryHandlerLogger;
}
