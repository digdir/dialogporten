using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Digdir.Library.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TodoCommentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TODO001";
    private static readonly LocalizableString Title = "TODO comment must have a GitHub issue URL";
    private static readonly LocalizableString MessageFormat = "TODO comment must be followed by a GitHub issue URL (on the same line)";
    private static readonly LocalizableString Description = "TODO comments should be followed by a GitHub issue URL (on the same line).";

    private const string Category = "Documentation";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }
    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var trivia = context
            .Tree
            .GetRoot(context.CancellationToken)
            .DescendantTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia));

        foreach (var comment in trivia)
        {
            var commentText = comment.ToString();

            if (!commentText.Contains("TODO")) continue;

            if (commentText.Contains("https://github.com/digdir/dialogporten/issues")) continue;

            var diagnostic = Diagnostic.Create(Rule, comment.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
