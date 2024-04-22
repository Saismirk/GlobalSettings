using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalSettingGenerator;

public class SyntaxContextReceiver : ISyntaxContextReceiver {
    internal static ISyntaxContextReceiver Create() => new SyntaxContextReceiver();
    public List<(ISymbol classSymbol, ClassDeclarationSyntax classSyntax)> Settings { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        var node = context.Node;

        if (node is not ClassDeclarationSyntax classDeclaration || classDeclaration.AttributeLists.Count == 0) {
            return;
        }

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol?.GetAttributes()
                       .Any(ad => ad.AttributeClass?.ToDisplayString() is "GlobalSettingsAttribute")
            == true) {
            Settings.Add((classSymbol, classDeclaration));
        }
    }
}