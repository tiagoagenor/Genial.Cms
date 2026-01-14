using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Genial.Cms.Domain.Exceptions;

namespace Genial.Cms.Domain.SeedWork;

public abstract class Enumeration
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var fields = typeof(T).GetFields(BindingFlags.Public
                                         | BindingFlags.Static | BindingFlags.DeclaredOnly);

        return fields.Select(f => f.GetValue(null)).Cast<T>();
    }

    private static T Parse<T>(Func<T, bool> predicate) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(predicate);
    }

    public static T FromName<T>(string name) where T : Enumeration
    {
        var state = Parse<T>(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for {typeof(T)}: {string.Join(",", GetAll<T>().Select(s => s.Name))}");
        }

        return state;
    }

    public static T FromId<T>(int id) where T : Enumeration
    {
        var state = Parse<T>(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for {typeof(T)}: {string.Join(",", GetAll<T>().Select(s => s.Id))}");
        }

        return state;
    }
}
