using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Templates;
internal class AssemblyTemplateProvider : ITemplateProvider
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
        return SyntaxFactory.ParseMemberDeclaration($"public string AssemblyName {{ get; }} = typeof({type}).Assembly.GetName().Name;");
    }
}
