using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrideSourceGenerator;
internal class ClassAttributeFinder
{
    List<String> allowedAttributes = new List<String>()
    { 
        "Stride.Core.DataContract",
        "DataContract",
        "System.Runtime.Serialization.DataContract"
    };
    public ClassDeclarationSyntax FindAttribute(SyntaxNode syntaxNode)
    {
        switch (syntaxNode)
        {
            case ClassDeclarationSyntax classDeclaration:
                var attribute = classDeclaration.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .FirstOrDefault(a => allowedAttributes.Contains(a.Name.ToString()));

                if (attribute != null)
                {
                    return classDeclaration;
                }
                break;
        }
        return null;
    }
}
