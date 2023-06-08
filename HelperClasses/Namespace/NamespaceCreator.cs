using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.HelperClasses.Namespace;
public class NamespaceCreator
{
    private NamespaceProvider NamespaceProvider { get; set; } = new();
    private UsingDirectiveProvider UsingDirectiveProvider { get; set; } = new();
    /// <summary>
    /// Creates a namespace for (the class that should be generated.
    /// Adds the using directives to the namespace in <see cref="UsingDirectiveProvider"/>
    /// Returns null if (it was not possible to create a namespace and will emit <see cref="DiagnosticDescriptor"/> message where it failed
    /// </summary>
    /// <param name="classDeclaration">The class which is the current context</param>
    /// <param name="context">The current execution of the Source Generator</param>
    /// <param name="className">Name of the class that is the current target of the generation.</param>
    /// <returns>a normal namespace or null if it failed.</returns>
    public NamespaceDeclarationSyntax CreateNamespace(ClassDeclarationSyntax classDeclaration,GeneratorExecutionContext context, string className)
    {
        var normalNamespace = NamespaceProvider.GetNamespaceFromSyntaxNode(classDeclaration);

        if (normalNamespace == null)
        {
            var error = NamespaceProvider.DiagnosticsErrorWhenNull(className);
            var location = Location.Create(classDeclaration.SyntaxTree, classDeclaration.Identifier.Span);
            context.ReportDiagnostic(Diagnostic.Create(error, location));
            return null;
        }
        normalNamespace = UsingDirectiveProvider.AddUsingDirectives(normalNamespace,classDeclaration,context,className);
        return normalNamespace;
    }
}
