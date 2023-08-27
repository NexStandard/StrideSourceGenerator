using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using StrideSourceGenerator.Core.Roslyn;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace StrideSourceGenerator.API;
/// <summary>
/// Provides utility methods for handling namespace-related operations during code generation.
/// </summary>
/// <typeparam name="T">The type of <c>TypeDeclarationSyntax</c> that the class handles.</typeparam>
sealed class NamespaceProvider<T>
    where T : TypeDeclarationSyntax
{
    public List<UsingDirective> Usings { get; set; } = new List<UsingDirective>()
    {
        new UsingDirective() {
            Name = " System",
            MetadataName = "System"
        },
        new UsingDirective() {
            Name = " VYaml.Parser",
            MetadataName = "VYaml.Parser",
        },
        new UsingDirective() {
            Name = " VYaml.Emitter",
            MetadataName = "VYaml.Emitter",
        },
        new UsingDirective() {
            Name = " VYaml.Serialization",
            MetadataName = "VYaml.Serialization"
        },
        new UsingDirective() {
            Name = " System.Text",
            MetadataName = "System.Text"
        }
    };
    /// <summary>
    /// Finds either a normal or a file-scoped namespace from a <see cref="SyntaxNode"/> by traversing its parent nodes.
    /// Then it creates a new namespace declaration.
    /// </summary>
    /// <param name="classInfo">The <see cref="ClassInfo{T}"/> containing information about the class and its context.</param>
    /// <returns>
    /// A new <see cref="NamespaceDeclarationSyntax"/>  or <c>null</c> if the <see cref="TypeSyntax"/> is declared in the default namespace.
    /// A diagnostics error will be reported that the default namespace is not allowed
    /// </returns>
    public NamespaceDeclarationSyntax GetNamespaceFromSyntaxNode(ClassInfo<T> classInfo)
    {
        SyntaxNode syntaxNode = classInfo.TypeSyntax;
        while (syntaxNode != null)
        {
            if (syntaxNode is BaseNamespaceDeclarationSyntax baseNamespace)
                return ConvertToNormalNamespace(baseNamespace);
            syntaxNode = syntaxNode.Parent;
        }
        ReportNoNamespaceError(classInfo.TypeSyntax, classInfo.ExecutionContext, classInfo.TypeName);
        return null;
    }
    /// <summary>
    /// Adds the <see cref="Usings"/> to the provided <see cref="NamespaceDeclarationSyntax"/>.
    /// </summary>
    /// <param name="normalNamespace">The <see cref="NamespaceDeclarationSyntax"/> to which using directives should be added.</param>
    /// <param name="context">The <see cref="GeneratorExecutionContext"/> used to access compilation information.</param>
    /// <returns>
    /// A modified <see cref="NamespaceDeclarationSyntax"/> with the added using directives.
    /// </returns>
    public NamespaceDeclarationSyntax AddUsingDirectivess(NamespaceDeclarationSyntax normalNamespace, GeneratorExecutionContext context)
    {
        foreach (UsingDirective allowedUsing in Usings)
        {
            normalNamespace = normalNamespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(allowedUsing.Name)));
        }
        return normalNamespace;
    }
    /// <summary>
    /// Reports a diagnostic error when a namespace declaration is missing for a given class.
    /// This method is used to generate a diagnostic when the provided class declaration doesn't belong to any namespace, which is only the case when its in the default namespace.
    /// </summary>
    /// <param name="classDeclaration">The <see cref="TypeDeclarationSyntax"/> representing the class without a namespace.</param>
    /// <param name="context">The <see cref="GeneratorExecutionContext"/> used to report diagnostics.</param>
    private static void ReportNoNamespaceError(TypeDeclarationSyntax classDeclaration, GeneratorExecutionContext context, string className)
    {
        DiagnosticDescriptor error = new DiagnosticDescriptor(
            id: "SYSG001",
            title: "Missing Namespace",
            messageFormat: $"The class {className} is not declared in a namespace. The global namespace is not allowed.",
            category: "Stride.Yaml.CompilerServices",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: $"The class {className} is not declared in a namespace. The class must be declared in a file-scoped or normal namespace. The global namespace is not allowed."
        );
        Location location = Location.Create(classDeclaration.SyntaxTree, classDeclaration.Identifier.Span);
        context.ReportDiagnostic(Diagnostic.Create(error, location));
    }
    /// <summary>
    /// Converts a given <see cref="BaseNamespaceDeclarationSyntax"/> into a normal <see cref="NamespaceDeclarationSyntax"/> instance.
    /// </summary>
    /// <param name="namespaceDeclaration">The <see cref="BaseNamespaceDeclarationSyntax"/> to be converted.</param>
    /// <returns>
    /// A <see cref="NamespaceDeclarationSyntax"/> instance representing the converted namespace,
    /// or <c>null</c> if the provided <paramref name="namespaceDeclaration"/> is <c>null</c>.
    /// </returns>
    private NamespaceDeclarationSyntax ConvertToNormalNamespace(BaseNamespaceDeclarationSyntax namespaceDeclaration)
    {
        if (namespaceDeclaration == null)
            return null;
        NameSyntax name = namespaceDeclaration.Name;
        return SyntaxFactory.NamespaceDeclaration(name);
    }

    public class UsingDirective
    {
        public string Name { get; set; }
        public string MetadataName { get; set; }
    }
}
