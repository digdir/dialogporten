using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;

internal sealed record PropertyInfoWithNullability(PropertyInfo Property, NullabilityInfo NullabilityInfo);
