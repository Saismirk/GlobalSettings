#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalSettingGenerator;

[Generator]
public class SettingGenerator : ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(SyntaxContextReceiver.Create);
    }

    public void Execute(GeneratorExecutionContext context) {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver) return;
        ProcessAttribute("GlobalSettings", context, receiver);
    }

    private static void ProcessAttribute(string attribute, GeneratorExecutionContext context, SyntaxContextReceiver receiver) {
        var attributeSymbol = context.Compilation.GetTypeByMetadataName($"{attribute}Attribute");
        if (attributeSymbol == null) {
#if DEBUG
            Utils.SaveSourceToPath("E:/Error.txt", $"Attribute not found: {attribute}");
#endif
            return;
        }
        foreach (var classData in receiver.Settings) {
            var classSource = ProcessClass(classData.classSymbol, attributeSymbol, classData.classSyntax);
            if (string.IsNullOrEmpty(classSource)) continue;
            var namespaceName = classData.classSymbol.ContainingNamespace?.Name ?? "global";
            var className = classData.classSymbol.Name;
            context.AddSource($"{namespaceName}_{className}_{attribute}.g.cs", classSource);
#if DEBUG
            Utils.SaveSourceToPath($"E:/{namespaceName}_{className}_{attribute}.g.txt", classSource);
#endif
        }
    }

    private static string ProcessClass(ISymbol classSymbol, ISymbol? attributeSymbol, ClassDeclarationSyntax classSyntax) {
        var source = new StringBuilder();

        try {
            if (!GenerateClass(classSymbol, attributeSymbol, classSyntax, source)) return string.Empty;
        }
        catch (Exception e) {
            source.AppendLine($@"/*Error: {e.Message}*/");
            source.AppendLine($@"/*{e.StackTrace}*/");
        }

        return source.ToString();
    }

    private static bool GenerateClass(
        ISymbol classSymbol,
        ISymbol? attributeSymbol,
        ClassDeclarationSyntax classSyntax,
        StringBuilder sourceBuilder) {
        var tempSettings = new List<GlobalSetting>();
        var attributeData = classSymbol.GetAttributes()
                                       .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
        if (attributeData == null) return false;

        var settingsPath = attributeData.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(settingsPath)) return false;

        var settingsFullPath = SettingsParser.ParseSettings(classSyntax,
                                                            settingsPath,
                                                            tempSettings);
        var namespaceSymbol = classSymbol.ContainingNamespace;
        if (!namespaceSymbol.IsGlobalNamespace) {
            sourceBuilder.AppendLine($@"using System.Collections.Generic;

namespace {namespaceSymbol} {{");
        }

        sourceBuilder.AppendLine($@"    public static partial class {classSymbol.Name} {{//From: {settingsFullPath}");
        switch (attributeSymbol?.Name) {
            case "GlobalSettingsAttribute":
                sourceBuilder.AppendLine($@"        //Detected Settings: {tempSettings.Count}");
                AppendSetting(sourceBuilder, tempSettings);
                break;
        }

        sourceBuilder.AppendLine(namespaceSymbol.IsGlobalNamespace ? "}" : "\t}");
        if (!namespaceSymbol.IsGlobalNamespace) {
            sourceBuilder.AppendLine("}");
        }

        return true;
    }

    private static void AppendSetting(StringBuilder sourceBuilder, List<GlobalSetting> tempSettings) {
        foreach (var setting in tempSettings) {
            try {
                GlobalSettingProvider.Generate(sourceBuilder, setting);
            }
            catch (Exception e) {
                sourceBuilder.AppendLine($@"/*Error: {e.Message}*/");
            }
        }
    }
}

public class GlobalSetting {
    public GlobalSettingType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Value { get; set; }

    public GlobalSetting(GlobalSettingType type, string name, string description, string value) {
        Type = type;
        Name = name;
        Description = description;
        Value = value;
    }
}