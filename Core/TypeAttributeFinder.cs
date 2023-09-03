using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrideSourceGenerator.Core;
internal class TypeAttributeFinder
{
    List<string> allowedAttributes = new List<string>()
    {
        "Stride.Core.DataContract",
        // Needs to be decided later if its the Stride.Core.DataContract or not, it cant be decided in the Syntax Receiver
        "DataContract"
    };
    public TypeDeclarationSyntax FindAttribute(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax typeSyntax)
        {
            AttributeSyntax attribute = typeSyntax.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .FirstOrDefault(a => allowedAttributes.Contains(a.Name.ToString()));
            return typeSyntax;
        }
        if (syntaxNode is StructDeclarationSyntax structDeclaration)
        {
            AttributeSyntax attribute = structDeclaration.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .FirstOrDefault(a => allowedAttributes.Contains(a.Name.ToString()));
            return structDeclaration;
        }
        return null;
    }
}
