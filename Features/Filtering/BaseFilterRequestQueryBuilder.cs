using System.Linq.Expressions;
using System.Reflection;
using FamilyTree.Entity;

namespace FamilyTree.Features.Filtering;

public static class BaseFilterRequestQueryBuilder<TEntity>
    where TEntity : class
{
    public static IQueryable<TEntity> ApplyFilters(
        IQueryable<TEntity> query,
        Dictionary<string, string>? filters,
        bool showArchived = false)
    {
        PropertyInfo? archivedProperty = typeof(TEntity).GetProperty(nameof(BaseEntity.Archived));

        if (archivedProperty != null && !showArchived)
        {
            ParameterExpression archivedParameter = Expression.Parameter(typeof(TEntity), "entity");
            MemberExpression archivedPropertyAccess = Expression.Property(archivedParameter, archivedProperty);
            ConstantExpression archivedFalseValue = Expression.Constant(false);
            BinaryExpression archivedEqualExpression = Expression.Equal(archivedPropertyAccess, archivedFalseValue);

            Expression<Func<TEntity, bool>> archivedLambda =
                Expression.Lambda<Func<TEntity, bool>>(archivedEqualExpression, archivedParameter);

            query = query.Where(archivedLambda);
        }

        if (filters == null || !filters.Any())
        {
            return query;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression? finalExpression = null;

        foreach (KeyValuePair<string, string> filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Value))
            {
                continue;
            }

            string[] propertyPath = filter.Key.Split('.');
            Type currentType = typeof(TEntity);
            Expression propertyAccess = parameter;
            PropertyInfo? lastProperty = null;

            foreach (string propertyName in propertyPath)
            {
                lastProperty = currentType.GetProperty(propertyName);

                if (lastProperty == null)
                {
                    throw new Exception($"Field {filter.Key} does not exist.");
                }

                propertyAccess = Expression.Property(propertyAccess, lastProperty);
                currentType = lastProperty.PropertyType;
            }

            if (lastProperty == null)
            {
                continue;
            }

            Expression currentExpression;

            if (currentType == typeof(string))
            {
                MethodInfo toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
                MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

                MethodCallExpression propertyToLowerExpression = Expression.Call(propertyAccess, toLowerMethod);
                ConstantExpression filterValueExpression = Expression.Constant(filter.Value.ToLower(), typeof(string));

                currentExpression = Expression.Call(propertyToLowerExpression, containsMethod, filterValueExpression);
            }
            else
            {
                if (!TryParseField(filter.Value, currentType, out object? parsedValue))
                {
                    continue;
                }

                ConstantExpression constantExpression = Expression.Constant(parsedValue, currentType);
                currentExpression = Expression.Equal(propertyAccess, constantExpression);
            }

            finalExpression = finalExpression == null
                ? currentExpression
                : Expression.AndAlso(finalExpression, currentExpression);
        }

        if (finalExpression != null)
        {
            Expression<Func<TEntity, bool>> lambda =
                Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);

            query = query.Where(lambda);
        }

        return query;
    }

    public static IQueryable<TEntity> ApplyTermFilter(
        IQueryable<TEntity> query,
        string? term,
        params string[] searchableProperties)
    {
        if (string.IsNullOrWhiteSpace(term) || searchableProperties.Length == 0)
        {
            return query;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression? finalExpression = null;

        foreach (string propertyName in searchableProperties)
        {
            PropertyInfo? propertyInfo = typeof(TEntity).GetProperty(propertyName);

            if (propertyInfo == null || propertyInfo.PropertyType != typeof(string))
            {
                continue;
            }

            MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
            MethodInfo toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

            MethodCallExpression propertyToLowerExpression = Expression.Call(propertyAccess, toLowerMethod);
            ConstantExpression termExpression = Expression.Constant(term.ToLower(), typeof(string));
            MethodCallExpression containsExpression =
                Expression.Call(propertyToLowerExpression, containsMethod, termExpression);

            Expression propertyNotNullExpression = Expression.NotEqual(
                propertyAccess,
                Expression.Constant(null, typeof(string)));

            Expression safeContainsExpression = Expression.AndAlso(propertyNotNullExpression, containsExpression);

            finalExpression = finalExpression == null
                ? safeContainsExpression
                : Expression.OrElse(finalExpression, safeContainsExpression);
        }

        if (finalExpression == null)
        {
            return query;
        }

        Expression<Func<TEntity, bool>> lambda =
            Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);

        return query.Where(lambda);
    }

    private static bool TryParseField(string value, Type targetType, out object? parsedValue)
    {
        parsedValue = null;

        if (targetType == typeof(string))
        {
            parsedValue = value;
            return true;
        }

        Type? underlyingType = Nullable.GetUnderlyingType(targetType);

        if (underlyingType != null)
        {
            return TryParseField(value, underlyingType, out parsedValue);
        }

        if (targetType.IsEnum)
        {
            if (Enum.TryParse(targetType, value, true, out object? enumValue))
            {
                parsedValue = enumValue;
                return true;
            }

            return false;
        }

        switch (Type.GetTypeCode(targetType))
        {
            case TypeCode.Int32:
                if (int.TryParse(value, out int parsedInt))
                {
                    parsedValue = parsedInt;
                    return true;
                }

                break;

            case TypeCode.Int64:
                if (long.TryParse(value, out long parsedLong))
                {
                    parsedValue = parsedLong;
                    return true;
                }

                break;

            case TypeCode.Decimal:
                if (decimal.TryParse(value, out decimal parsedDecimal))
                {
                    parsedValue = parsedDecimal;
                    return true;
                }

                break;

            case TypeCode.Double:
                if (double.TryParse(value, out double parsedDouble))
                {
                    parsedValue = parsedDouble;
                    return true;
                }

                break;

            case TypeCode.Single:
                if (float.TryParse(value, out float parsedFloat))
                {
                    parsedValue = parsedFloat;
                    return true;
                }

                break;

            case TypeCode.Boolean:
                if (bool.TryParse(value, out bool parsedBoolean))
                {
                    parsedValue = parsedBoolean;
                    return true;
                }

                break;

            case TypeCode.DateTime:
                if (DateTime.TryParse(value, out DateTime parsedDateTime))
                {
                    parsedValue = parsedDateTime;
                    return true;
                }

                break;
        }

        return false;
    }
}
