using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.Core.Methods;
using StrideSourceGenerator.Core.Namespace;
using StrideSourceGenerator.Core.Properties;
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
    protected ConvertToYamlMethodFactory writerFactory;
    protected DeserializeMethodFactory DeserializeMethodFactory = new();
    protected abstract string GeneratorClassPrefix { get; }

    /// <summary>
    /// After <see cref="StartCreation"/> created the hull of the Generated Class, this will get called to implement Details of the Generated class.
    /// Like Methods, Properties etc...
    /// Everything that is inside the class.
    /// </summary>
    /// <param name="context">The <see cref="GeneratorExecutionContext"/> of the <see cref="ISourceGenerator"/></param>
    /// <param name="generatorClass">The GeneratedSerializer class, which will be altered and added to the Source</param>
    /// <param name="contextClass">The class that is in the current <paramref name="context"/> iteration</param>
    /// <returns>The altered version of the Generated class, with its body filled.</returns>
    protected abstract ClassDeclarationSyntax CreateGenerator(GeneratorExecutionContext context, ClassDeclarationSyntax generatorClass, T contextClass);

    public bool StartCreation(GeneratorExecutionContext context, T contextClass, BFNNexSyntaxReceiver syntaxReceiver)
    {
        var className = GetIdentifierName(contextClass);

        var serializerClassName = $"{GeneratorClassPrefix}{className}";
        var generatorClass = SyntaxFactory.ClassDeclaration(serializerClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));


        // Create a namespace for the new class, if its not possible then the GeneratedSerializer can't be created
        NamespaceDeclarationSyntax normalNamespace = NamespaceCreator.CreateNamespace(contextClass, context, className);
        if (normalNamespace == null)
            return false;

        generatorClass = CreateGenerator(context, generatorClass, contextClass);

        generatorClass = AddInterfaces(generatorClass, className);
        var compilationUnit = SyntaxFactory.CompilationUnit()
                                              .AddMembers(normalNamespace.AddMembers(generatorClass));

        var sourceText = compilationUnit.NormalizeWhitespace().ToFullString();

        string namespaceName = normalNamespace.Name.ToString();
        context.AddSource($"{serializerClassName}_{namespaceName}.g.cs", sourceText);
        return true;
    }
    protected string GetIdentifierName(TypeDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Identifier.ValueText;
    }
    protected ClassDeclarationSyntax AddInterfaces(ClassDeclarationSyntax partialClass, string className)
    {
        return partialClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlSerializer<{className}>")));
    }
}
