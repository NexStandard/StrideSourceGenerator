using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.CodeAnalysis.CSharp;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Roslyn;

namespace StrideSourceGenerator.Core.Templates;
internal class TagTemplateProvider : ITemplateProvider
{
    public MemberDeclarationSyntax GetTemplate(ClassInfo classInfo)
    {
        string type = classInfo.TypeName;
        if (classInfo.IsGeneric)
        {
            TypeParameterListSyntax generics = classInfo.Generics;
            int count = generics.Parameters.Count;
            string emptyGenerics = "<" + new string(',', count - 1) + ">";
            type += emptyGenerics;
        }
        return SyntaxFactory.ParseMemberDeclaration($"public string IdentifierTag {{ get; }} = typeof({type}).Name.Replace('`','$';");
    }
}
