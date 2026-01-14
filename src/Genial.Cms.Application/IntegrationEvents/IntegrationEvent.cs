using System;
using MediatR;

namespace Genial.Cms.Application.IntegrationEvents;

public abstract class IntegrationEvent : INotification
{
    public DateTime TimeStamp { get; }

    protected IntegrationEvent()
    {
        TimeStamp = DateTime.UtcNow;
    }
}
