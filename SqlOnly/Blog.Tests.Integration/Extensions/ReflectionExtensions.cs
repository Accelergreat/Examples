#nullable enable
using System.Reflection;

namespace Blog.Tests.Integration.Extensions;

public static class ReflectionExtensions
{
    public static T SetThroughReflection<T>(this T entity, string propertyName, object? value)
    {
        var properties = typeof(T).GetProperties(BindingFlags.FlattenHierarchy |
                                                 BindingFlags.Instance |
                                                 BindingFlags.Public).ToArray();

        var named = properties.Where(x => x.Name == propertyName).ToArray();

        var property = typeof(T).GetProperties(BindingFlags.FlattenHierarchy |
                                               BindingFlags.Instance |
                                               BindingFlags.Public)
            .Single(s => s.SetMethod != null && s.Name == propertyName);

        property.SetValue(entity, value);

        return entity;
    }
}