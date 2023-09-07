using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.API;
interface ITemplateProvider
{
    public MemberDeclarationSyntax GetTemplate(ClassInfo value);
}
