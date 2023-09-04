using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Methods.RegisterTemplates;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods;
internal class RegisterMethodFactory
{
    IAppendableTemplate itselfRegister = new ItselfRegisterTemplate();
    IAppendableTemplate interfaceRegister = new InterfaceRegisterTemplate();
    IAppendableTemplate abstractRegister = new AbstractClassRegister();
    public MemberDeclarationSyntax GetRegisterMethod(ClassInfo classInfo)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("public void Register(){");
        itselfRegister.AppendTemplate(classInfo, builder);
        interfaceRegister.AppendTemplate(classInfo, builder);
        abstractRegister.AppendTemplate(classInfo, builder);
        builder.AppendLine("}");
        return SyntaxFactory.ParseMemberDeclaration(builder.ToString());
    }
}
