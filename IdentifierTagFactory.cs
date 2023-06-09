using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator;
internal class IdentifierTagFactory
{
    public string IdentifierTagTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols)
    {
        return $"public string IdentifierTag {{ get; }} = \"!{className}\";";
    }
}
internal class IdentifierTypeFactory
{
    public string IdentifierTagTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols)
    {
        return $"public Type IdentifierType {{ get; }} = typeof({className});";
    }
}
