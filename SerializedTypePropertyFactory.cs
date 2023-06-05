using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator;
internal class SerializedTypePropertyFactory
{
    public string SerializedTypeProperty(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols)
    {
        return $"public Type SerializedType => typeof({className});";
    }
}
