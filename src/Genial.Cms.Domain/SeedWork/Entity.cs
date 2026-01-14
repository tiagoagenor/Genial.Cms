using System;
using System.Collections.Generic;
using MediatR;

namespace Genial.Cms.Domain.SeedWork;

public abstract class Entity
{
    public virtual Guid Id { get; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    #region Entity

    private bool IsTransient()
    {
        return Id == default;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Entity item)
            return false;

        if (ReferenceEquals(this, item))
            return true;

        if (GetType() != item.GetType())
            return false;

        if (item.IsTransient() || IsTransient())
            return false;

        return item.Id == this.Id;
    }

    public override int GetHashCode()
    {
        if (IsTransient()) return default;

        return Id.GetHashCode() ^ 31;
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return left?.Equals(right) ?? Equals(right, null);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }

    #endregion

    #region DomainEvents

    private List<INotification> _domainEvents;
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents ??= new List<INotification>();
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    public void ClearDomainEvent()
    {
        _domainEvents?.Clear();
    }

    #endregion
}
