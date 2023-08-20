using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace StrideSourceGenerator;
internal class StructAttributeFinder
{
    List<String> allowedAttributes = new List<String>()
    {
        "Stride.Core.DataContract",
        "DataContract",
        "System.Runtime.Serialization.DataContract"
    };
        public StructDeclarationSyntax FindAttribute(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case StructDeclarationSyntax classDeclaration:
                AttributeSyntax attribute = classDeclaration.AttributeLists
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
