using System.Linq.Expressions;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal static class Expressions
{
    internal static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    internal static Expression<Func<Localization, bool>> LocalizedSearchExpression(string? search, string? languageCode)
    {
        return localization =>
            (languageCode == null || localization.LanguageCode == languageCode) &&
            EF.Functions.ILike(localization.Value, $"%{search}%");
    }
}
