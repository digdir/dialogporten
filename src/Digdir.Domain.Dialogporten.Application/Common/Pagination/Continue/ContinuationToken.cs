using System.Linq.Expressions;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Continue;

public class ContinuationToken
{
    private static readonly MemberInfo ValueMemberInfo = typeof(ContinuationToken).GetProperty(nameof(Value))!;
    private readonly Type _type;
    public string Key { get; }
    public object? Value { get; }

    public ContinuationToken(string key, object? value, Type type)
    {
        Key = key;
        Value = value;
        _type = type;
    }

    public Expression GetValueExpression() => Expression.Convert(Expression.MakeMemberAccess(Expression.Constant(this), ValueMemberInfo), _type);
}
