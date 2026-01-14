using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Genial.Cms.Domain.Exceptions;

public class ExceptionNotificationHandler : INotificationHandler<ExceptionNotification>
{
    private ICollection<ExceptionNotification> _notifications;

    public ExceptionNotificationHandler()
    {
        _notifications = new List<ExceptionNotification>();
    }

    public Task Handle(ExceptionNotification message, CancellationToken cancellationToken)
    {
        _notifications.Add(message);

        return Task.CompletedTask;
    }

    public virtual ICollection<ExceptionNotification> GetNotifications()
    {
        return _notifications;
    }

    public ExceptionType GetExceptionType()
    {
        var notification = _notifications.FirstOrDefault();

        return notification?.Type ?? default;
    }

    public virtual bool HasNotifications()
    {
        return GetNotifications().Any();
    }

    public void Dispose()
    {
        _notifications = new List<ExceptionNotification>();
    }
}
