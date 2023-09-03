using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.CodeAnalysis.CSharp;
using StrideSourceGenerator.API;

namespace StrideSourceGenerator.Core.Templates;
internal class TagTemplateProvider : ITemplateProvider
{
    public MemberDeclarationSyntax GetTemplate(string className)
    {
        return SyntaxFactory.ParseMemberDeclaration($"public string IdentifierTag {{ get; }} = typeof({className}).Name.Replace('`','$';");
    }
}
