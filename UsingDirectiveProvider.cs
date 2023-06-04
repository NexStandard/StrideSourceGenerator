using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace StrideSourceGenerator;
internal class UsingDirectiveProvider
{
    List<String> allowedAttributes = new List<String>()
    {
        " System.Xml.Serialization",
        " System.Xml",
        " Stride.Core",
        " YamlDotNet.Serialization"
    };
    public NamespaceDeclarationSyntax AddUsingDirectives(NamespaceDeclarationSyntax normalNamespace)
    {
        allowedAttributes.ForEach(x =>normalNamespace = normalNamespace.AddUsings( SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(x))));

        return normalNamespace;
    }
}
