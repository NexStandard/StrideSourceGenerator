using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using StrideSourceGenerator.Core;

namespace StrideSourceGenerator;

public class NexSyntaxReceiver : ISyntaxReceiver
{
    TypeAttributeFinder _typeFinder = new();
    public List<TypeDeclarationSyntax> TypeDeclarations { get; private set; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        TypeDeclarationSyntax result = _typeFinder.FindAttribute(syntaxNode);

        if (result != null)
        {
            TypeDeclarations.Add(result);
        }
    }
}
