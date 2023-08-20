using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.API;
public interface IAttributeFinder
{
    List<string> AllowedAttributes { get; }
    ClassDeclarationSyntax FindAttribute(SyntaxNode syntaxNode);
}
