using System.Linq.Expressions;
using System.Reflection;
using VoxBox.Api.Models;

namespace VoxBox.Api.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, SortDirection sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        
        if (property == null)
        {
            return query;
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.PropertyType],
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var filterParts = filter.Split(':', 2);
        
        if (filterParts.Length != 2)
        {
            return query;
        }

        var propertyName = filterParts[0].Trim();
        var propertyValue = filterParts[1].Trim();

        var property = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        
        if (property == null)
        {
            return query;
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        Expression filterExpression;

        if (property.PropertyType == typeof(string))
        {
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
            
            if (toLowerMethod != null && containsMethod != null)
            {
                var toLowerExpression = Expression.Call(propertyAccess, toLowerMethod);
                var valueExpression = Expression.Constant(propertyValue.ToLower());
                filterExpression = Expression.Call(toLowerExpression, containsMethod, valueExpression);
            }
            else
            {
                return query;
            }
        }
        else if (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?))
        {
            if (Guid.TryParse(propertyValue, out var guidValue))
            {
                filterExpression = Expression.Equal(propertyAccess, Expression.Constant(guidValue, property.PropertyType));
            }
            else
            {
                return query;
            }
        }
        else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
        {
            if (bool.TryParse(propertyValue, out var boolValue))
            {
                filterExpression = Expression.Equal(propertyAccess, Expression.Constant(boolValue, property.PropertyType));
            }
            else
            {
                return query;
            }
        }
        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
        {
            if (int.TryParse(propertyValue, out var intValue))
            {
                filterExpression = Expression.Equal(propertyAccess, Expression.Constant(intValue, property.PropertyType));
            }
            else
            {
                return query;
            }
        }
        else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
        {
            if (decimal.TryParse(propertyValue, out var decimalValue))
            {
                filterExpression = Expression.Equal(propertyAccess, Expression.Constant(decimalValue, property.PropertyType));
            }
            else
            {
                return query;
            }
        }
        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
        {
            if (DateTime.TryParse(propertyValue, out var dateValue))
            {
                filterExpression = Expression.Equal(propertyAccess, Expression.Constant(dateValue, property.PropertyType));
            }
            else
            {
                return query;
            }
        }
        else
        {
            return query;
        }

        var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
        return query.Where(lambda);
    }

    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, string? filters)
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return query;
        }

        var filterParts = filters.Split(';', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var filter in filterParts)
        {
            query = query.ApplyFilter(filter.Trim());
        }

        return query;
    }
}
