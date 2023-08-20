using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace StrideSourceGenerator.Core.Namespace;
sealed class NamespaceProvider
{
    /// <summary>
    /// Finds either the Normalnamespace or the Filescoped namespace from a syntaxnode by looping over the parents.
    /// Then it creates a new one, as removing the classes in the scope is too tedious.
    /// </summary>
    /// <param name="syntaxNode">The syntax node that should be scanned for (a namespace</param>
    /// <returns>A new namespace that can be used in the generated code</returns>
    public NamespaceDeclarationSyntax GetNamespaceFromSyntaxNode(SyntaxNode syntaxNode)
    {
        NamespaceProvider provider = new();
        while (syntaxNode != null)
        {
            if (syntaxNode is BaseNamespaceDeclarationSyntax baseNamespace)
                return provider.ConvertToNormalNamespace(baseNamespace);
            syntaxNode = syntaxNode.Parent;
        }

        return null;
    }
    public static DiagnosticDescriptor DiagnosticsErrorWhenNull(string className)
    {
        return new DiagnosticDescriptor(
            id: "SYSG001",
            title: "Missing Namespace",
            messageFormat: $"The class {className} is not declared in a namespace. The global namespace is not allowed.",
            category: "Stride.Yaml.CompilerServices",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: $"The class {className} is not declared in a namespace. The class must be declared in a file-scoped or normal namespace. The global namespace is not allowed."
        );
    }
    private NamespaceDeclarationSyntax ConvertToNormalNamespace(BaseNamespaceDeclarationSyntax namespaceDeclaration)
    {
        if (namespaceDeclaration == null) return null;
        if (namespaceDeclaration is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
            return CreateFileScopedNamespace(fileScopedNamespace);
        if (namespaceDeclaration is NamespaceDeclarationSyntax normalNamespace)
            return CreateNormalNamespace(normalNamespace);
        return null;
    }
    private NamespaceDeclarationSyntax CreateFileScopedNamespace(FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
    {
        NameSyntax name = fileScopedNamespace.Name;
        return SyntaxFactory.NamespaceDeclaration(name);
    }
    private NamespaceDeclarationSyntax CreateNormalNamespace(NamespaceDeclarationSyntax normalNamespace)
    {
        NameSyntax name = normalNamespace.Name;
        return SyntaxFactory.NamespaceDeclaration(name);
    }
}
