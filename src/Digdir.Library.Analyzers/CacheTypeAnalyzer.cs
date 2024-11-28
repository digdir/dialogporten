using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Digdir.Library.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CacheTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticIdNoTypeArgument = "CACH001";
    public const string DiagnosticIdNotImmutableRecord = "CACH002";

    private static readonly LocalizableString TitleNoTypeArgument = "IFusionCache.GetOrSetAsync cannot be used without a type argument";
    private static readonly LocalizableString MessageFormatNoTypeArgument = "IFusionCache.GetOrSetAsync cannot be used without a type argument";
    private static readonly LocalizableString DescriptionNoTypeArgument = "Ensure that IFusionCache.GetOrSetAsync is used with a type argument.";

    private static readonly LocalizableString TitleNotImmutableRecord = "Cache type must be a readonly type";
    private static readonly LocalizableString MessageFormatNotImmutableRecord = "The type used in the cache must be a readonly, but was {0}";
    private static readonly LocalizableString DescriptionNotImmutableRecord = "Ensure that the types stored in the caches are readonly.";

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor RuleNoTypeArgument = new(
        DiagnosticIdNoTypeArgument, TitleNoTypeArgument, MessageFormatNoTypeArgument,
        Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: DescriptionNoTypeArgument);

    private static readonly DiagnosticDescriptor RuleNotImmutableRecord = new(
        DiagnosticIdNotImmutableRecord, TitleNotImmutableRecord, MessageFormatNotImmutableRecord,
        Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: DescriptionNotImmutableRecord);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [RuleNoTypeArgument, RuleNotImmutableRecord];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
        {
            return;
        }

        if (memberAccessExpr.Name.Identifier.Text != "GetOrSetAsync")
        {
            return;
        }

        if (memberAccessExpr.Name is not GenericNameSyntax genericName)
        {
            // Report diagnostic if there is no generic type argument
            var diagnostic = Diagnostic.Create(RuleNoTypeArgument, memberAccessExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var genericTypeArgument = genericName.TypeArgumentList.Arguments[0];
        var semanticModel = context.SemanticModel;
        var typeSymbol = semanticModel.GetTypeInfo(genericTypeArgument).Type;

        if (typeSymbol != null && IsImmutable(typeSymbol)) return;
        {
            var diagnostic = Diagnostic.Create(RuleNotImmutableRecord, genericTypeArgument.GetLocation(), typeSymbol?.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsImmutable(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType == SpecialType.System_String)
        {
            return true;
        }

        return typeSymbol.IsReadOnly && typeSymbol.GetMembers().OfType<IPropertySymbol>().All(p => p.IsReadOnly);
    }
}
