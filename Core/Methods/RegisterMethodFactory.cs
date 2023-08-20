using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods;
internal class RegisterMethodFactory
{
    public MemberDeclarationSyntax GetRegisterMethod<T>(ClassInfo<T> classInfo)
        where T : TypeDeclarationSyntax
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("public void Register(){");

        AddInterfaces(classInfo, builder);
        AddAbstractClasses(classInfo, builder);

        builder.AppendLine("}");
        return SyntaxFactory.ParseMemberDeclaration(builder.ToString());
    }
    protected void AddInterfaces<T>(ClassInfo<T> classInfo,StringBuilder builder)
        where T : TypeDeclarationSyntax
    {
        ITypeSymbol classSymbol = classInfo.Symbol;

        // Get the interfaces implemented by the class
        System.Collections.Immutable.ImmutableArray<INamedTypeSymbol> interfaces = classSymbol.AllInterfaces;
        builder.AppendLine("NexYamlSerializerRegistry.Default.RegisterFormatter(this);");
        foreach (INamedTypeSymbol interfacei in interfaces)
        {
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterInterface(this,typeof({interfacei.Name}));");
        }
    }
    protected void AddAbstractClasses<T>(ClassInfo<T> classInfo, StringBuilder builder)
        where T : TypeDeclarationSyntax
    {
        ITypeSymbol classSymbol = classInfo.Symbol;
        if(classSymbol.IsAbstract)
        {
            return;
        }
        INamedTypeSymbol currentBaseType = classSymbol.BaseType;
        while(currentBaseType != null)
        {
            if(currentBaseType.IsAbstract)
            {
                builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterAbstractClass(this,typeof({currentBaseType.Name}));");
            }
            currentBaseType = currentBaseType.BaseType;
        }
    }
}
