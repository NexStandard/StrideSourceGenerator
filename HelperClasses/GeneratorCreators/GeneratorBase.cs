using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.HelperClasses.Methods;
using StrideSourceGenerator.HelperClasses.Namespace;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.HelperClasses.GeneratorCreators;
internal abstract class GeneratorBase<T>
    where T : TypeDeclarationSyntax
{
    protected IdentifierTagFactory SerializedTypePropertyFactory = new();
    protected IdentifierTypeFactory IdentifierTypeFactory = new();
    protected NamespaceCreator NamespaceCreator { get; } = new();

    public abstract void CreateGenerator(GeneratorExecutionContext context, T partialClass, BFNNexSyntaxReceiver syntaxReceiver);

    protected string GetIdentifierName(T classDeclaration)
    {
        return classDeclaration.Identifier.ValueText;
    }
    protected ClassDeclarationSyntax AddInterfaces(ClassDeclarationSyntax partialClass, string className)
    {
        return partialClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlSerializer<{className}>")));
    }
}
