using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static System.Text.RegularExpressions.Regex;

namespace Digdir.Library.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TodoCommentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TODO001";
    private static readonly LocalizableString Title = "TODO comment must have an issue URL";
    private static readonly LocalizableString MessageFormat = "TODO comment must be followed by an issue URL (on the same line)";
    private static readonly LocalizableString Description = "TODO comments should be followed by an issue URL (on the same line).";

    private const string Category = "Documentation";
    private const string IssueRegexOptionKey = "dotnet_diagnostic.TODO001.issueRegex";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }
    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
        if (!options.TryGetValue(IssueRegexOptionKey, out var issueRegex) || string.IsNullOrEmpty(issueRegex))
        {
            return;
        }

        var trivia = context
            .Tree
            .GetRoot(context.CancellationToken)
            .DescendantTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia));

        foreach (var comment in trivia)
        {
            var commentText = comment.ToString();

            if (!commentText.Contains("TODO")) continue;

            if (IsMatch(commentText, issueRegex)) continue;

            var diagnostic = Diagnostic.Create(Rule, comment.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}