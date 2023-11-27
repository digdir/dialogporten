using System.Linq.Expressions;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal static class Expressions
{
    internal static class Boolean<T>
    {
        public static readonly Expression<Func<T, bool>> True = x => true;
        public static readonly Expression<Func<T, bool>> False = x => false;
    }

    internal static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    internal static Expression<Func<Localization, bool>> LocalizedSearchExpression(string? search, string? cultureCode)
    {
        return localization =>
            (cultureCode == null || localization.CultureCode == cultureCode) &&
            EF.Functions.ILike(localization.Value, $"%{search}%");
    }
}
