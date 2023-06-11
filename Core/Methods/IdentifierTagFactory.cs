using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods;
internal class IdentifierTagFactory
{
    public string IdentifierTagTemplate(string className)
    {
        return $"public string IdentifierTag {{ get; }} = \"!{className}\";";
    }
}
internal class IdentifierTypeFactory
{
    public string IdentifierTagTemplate(string className)
    {
        return $"public Type IdentifierType {{ get; }} = typeof({className});";
    }
}
