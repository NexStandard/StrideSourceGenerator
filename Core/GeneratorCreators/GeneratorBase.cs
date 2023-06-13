using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.Core.Methods;
using StrideSourceGenerator.Core.Namespace;
using StrideSourceGenerator.Core.Properties;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.GeneratorCreators;

internal abstract class GeneratorBase<T>
    where T : TypeDeclarationSyntax
{
    protected IdentifierTagFactory SerializedTypePropertyFactory = new();
    protected IdentifierTypeFactory IdentifierTypeFactory = new();
    protected PropertyAttributeFinder PropertyFinder { get; } = new();
    protected NamespaceCreator NamespaceCreator { get; } = new();
    protected SerializeMethodFactory writerFactory;
    protected DeserializeMethodFactory DeserializeMethodFactory = new();
    protected abstract string GeneratorClassPrefix { get; }
    
    public bool StartCreation(ClassInfo<T> classInfo)
    {
        classInfo.TypeName = GetIdentifierName(classInfo.TypeSyntax);
        classInfo.SerializerName = GeneratorClassPrefix + classInfo.TypeName;

        classInfo.SerializerSyntax = SyntaxFactory.ClassDeclaration(classInfo.SerializerName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        NamespaceDeclarationSyntax normalNamespace = NamespaceCreator.CreateNamespace(classInfo.TypeSyntax,classInfo.ExecutionContext, classInfo.TypeName);
        if (normalNamespace == null)
            return false;
               classInfo.SerializerSyntax = CreateGenerator(classInfo);

       classInfo.SerializerSyntax = AddInterfaces(classInfo.SerializerSyntax, classInfo.TypeName);

        var compilationUnit = SyntaxFactory.CompilationUnit()
                                      .AddMembers(normalNamespace
                                      .AddMembers(classInfo.SerializerSyntax));

        var sourceText = compilationUnit
            .NormalizeWhitespace()
            .ToFullString();

        string namespaceName = normalNamespace.Name.ToString();
        classInfo.ExecutionContext.AddSource($"{classInfo.SerializerName}_{namespaceName}.g.cs", sourceText);
        return true;
    }
    protected abstract ClassDeclarationSyntax CreateGenerator(ClassInfo<T> classInfo);

    protected string GetIdentifierName(TypeDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Identifier.ValueText;
    }
    protected T AddMember(T context,string newMember)
    {
        context = (T)context.AddMembers(SyntaxFactory.ParseMemberDeclaration(newMember));
        return context;
    }
    protected ClassDeclarationSyntax AddInterfaces(ClassDeclarationSyntax partialClass, string className)
    {
        return partialClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlFormatter<{className}?>")));
    }
    protected string RegistrationInResolver(string generatorName)
    {
        return $$"""
        static {{generatorName}}()
        {
            System.Console.WriteLine("The static constructor invoked.");
            global::VYaml.Serialization.GeneratedResolver.Register(new {{generatorName}}());
        }
        """;
    }
}
