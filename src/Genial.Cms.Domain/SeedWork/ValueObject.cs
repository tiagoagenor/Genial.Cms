using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Genial.Cms.Domain.SeedWork;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public static bool operator ==(ValueObject obj1, ValueObject obj2)
    {
        return obj1?.Equals(obj2) ?? Equals(obj2, null);
    }

    public static bool operator !=(ValueObject obj1, ValueObject obj2)
    {
        return !(obj1 == obj2);
    }

    public override bool Equals(object obj)
    {
        return Equals((ValueObject)obj);
    }

    public bool Equals(ValueObject obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;

        return GetProperties().All(p => PropertiesAreEqual(obj, p))
               && GetFields().All(f => FieldsAreEqual(obj, f));
    }

    private bool PropertiesAreEqual(object obj, PropertyInfo p)
    {
        return Equals(p.GetValue(this, null), p.GetValue(obj, null));
    }

    private bool FieldsAreEqual(object obj, FieldInfo f)
    {
        return Equals(f.GetValue(this), f.GetValue(obj));
    }

    private IEnumerable<PropertyInfo> GetProperties()
    {
        return GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToList();
    }

    private IEnumerable<FieldInfo> GetFields()
    {
        return GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ToList();
    }

    public override int GetHashCode()
    {
        var hash = GetProperties()
            .Select(prop => prop.GetValue(this, null))
            .Aggregate(17, HashValue);

        return GetFields()
            .Select(field => field.GetValue(this))
            .Aggregate(hash, HashValue);
    }

    private static int HashValue(int seed, object value)
    {
        var currentHash = value?.GetHashCode() ?? 0;

        return seed * 23 + currentHash;
    }
}
