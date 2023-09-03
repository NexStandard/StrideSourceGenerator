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
    public MemberDeclarationSyntax GetRegisterMethod(ClassInfo classInfo)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("public void Register(){");
        AddItself(classInfo,builder);
        AddInterfaces(classInfo, builder);
        AddAbstractClasses(classInfo, builder);

        builder.AppendLine("}");
        return SyntaxFactory.ParseMemberDeclaration(builder.ToString());
    }

    private void AddItself(ClassInfo classInfo, StringBuilder builder) 
    {
        if(classInfo.Generics != null && classInfo.Generics.Parameters.Count > 0)
        {
            string str = new string(',', classInfo.Generics.Parameters.Count-1);
            string generic = $"{classInfo.SerializerSyntax.Identifier.Text}<{str}>";
            string genericOfType = $"{classInfo.TypeName}<{str}>";
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterGenericFormatter(typeof({genericOfType}),typeof({generic}));");
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterFormatter(typeof({classInfo.TypeName+"<"+str+">"}));");
        }
        else
        {
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterFormatter(this);");
        }
    }

    protected void AddInterfaces(ClassInfo classInfo,StringBuilder builder)
    {
        ITypeSymbol classSymbol = classInfo.Symbol;

        // Get the interfaces implemented by the class
        System.Collections.Immutable.ImmutableArray<INamedTypeSymbol> interfaces = classSymbol.AllInterfaces;
        foreach (INamedTypeSymbol interfacei in interfaces)
        {
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterInterface(this,typeof({interfacei.Name}));");
        }
    }
    protected void AddAbstractClasses(ClassInfo classInfo, StringBuilder builder)
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
