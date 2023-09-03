using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using StrideSourceGenerator.Core;

namespace StrideSourceGenerator;

public class NexSyntaxReceiver : ISyntaxReceiver
{
    TypeAttributeFinder typeFinder = new();
    public List<TypeDeclarationSyntax> ClassDeclarations { get; private set; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        TypeDeclarationSyntax result = typeFinder.FindAttribute(syntaxNode);

        if (result != null)
        {
            ClassDeclarations.Add(result);
        }
    }
}
