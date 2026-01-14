using System.Threading;
using MediatR;
using System.Threading.Tasks;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Application.Commands;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers;

public abstract class CommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : Command<TResponse>
{
    protected readonly IMediator Bus;
    protected readonly ILogger Logger;

    private readonly IUnitOfWork _uow;
    private readonly ExceptionNotificationHandler _notifications;

    protected CommandHandler(IUnitOfWork uow, IMediator bus, INotificationHandler<ExceptionNotification> notifications, ILogger logger)
    {
        Bus = bus;
        Logger = logger;

        _uow = uow;
        _notifications = (ExceptionNotificationHandler) notifications;
    }

    public async Task<bool> CommitAsync()
    {
        if (_notifications.HasNotifications()) return false;
        if (await _uow.CommitAsync()) return true;

        Logger.LogCritical("Problem on saving changes in database");
        await Bus.Publish(new ExceptionNotification("002", "We had a problem during saving your data.", ExceptionType.Server));

        return false;
    }

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
