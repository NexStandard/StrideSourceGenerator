using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Namespace;
public class NamespaceCreator
{
    private NamespaceProvider NamespaceProvider { get; set; } = new();
    /// <summary>
    /// Creates a namespace for (the class that should be generated.
    /// Adds the using directives to the namespace in <see cref="UsingDirectiveProvider"/>
    /// Returns null if (it was not possible to create a namespace and will emit <see cref="DiagnosticDescriptor"/> message where it failed
    /// </summary>
    /// <param name="classDeclaration">The class which is the current context</param>
    /// <param name="context">The current execution of the Source Generator</param>
    /// <param name="className">Name of the class that is the current target of the generation.</param>
    /// <returns>a normal namespace or null if it failed.</returns>
    public NamespaceDeclarationSyntax CreateNamespace(TypeDeclarationSyntax classDeclaration, GeneratorExecutionContext context, string className)
    {
        var normalNamespace = NamespaceProvider.GetNamespaceFromSyntaxNode(classDeclaration);

        if (normalNamespace == null)
        {
            var error = NamespaceProvider.DiagnosticsErrorWhenNull(className);
            var location = Location.Create(classDeclaration.SyntaxTree, classDeclaration.Identifier.Span);
            context.ReportDiagnostic(Diagnostic.Create(error, location));
            return null;
        }
        normalNamespace = AddUsingDirectives(normalNamespace, context);
        return normalNamespace;
    }
    List<UsingDirective> allowedAttributes = new List<UsingDirective>()
    {
        new UsingDirective() {
            Name = " System",
            MetadataName = "System",
            NugetReference = "",
            HelpLink = "https://stride-docs-test.azurewebsites.net/latest"
        },
        new UsingDirective() {
            Name = " VYaml.Parser",
            MetadataName = "VYaml.Parser",
            NugetReference = "",
            HelpLink = ""
        },
        new UsingDirective() {
            Name = " VYaml.Emitter",
            MetadataName = "VYaml.Emitter",
            NugetReference = "",
            HelpLink = ""
        },
        new UsingDirective() {
            Name = " VYaml.Serialization",
            MetadataName = "VYaml.Serialization",
            NugetReference = "",
            HelpLink = ""
        },
        new UsingDirective() {
            Name = " System.Text",
            MetadataName = "System.Text",
            NugetReference = "",
            HelpLink = ""
        }
    };
    public NamespaceDeclarationSyntax AddUsingDirectives(NamespaceDeclarationSyntax normalNamespace, GeneratorExecutionContext context)
    {

        var compilation = context.Compilation;
        foreach (var allowedUsing in allowedAttributes)
        {
            normalNamespace = normalNamespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(allowedUsing.Name)));
        }
        return normalNamespace;
    }
    private class UsingDirective
    {
        public string Name { get; set; }
        public string MetadataName { get; set; }
        public string NugetReference { get; set; }
        public string HelpLink { get; set; }
    }
}
