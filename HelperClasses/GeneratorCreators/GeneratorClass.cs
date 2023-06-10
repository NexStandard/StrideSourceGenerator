using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using StrideSourceGenerator.HelperClasses.Namespace;
using System;
using System.Collections.Generic;
using System.Text;
using StrideSourceGenerator.HelperClasses.Methods;
using StrideSourceGenerator.HelperClasses.Properties;
using System.Linq;

namespace StrideSourceGenerator.HelperClasses.GeneratorCreators;
internal class GeneratorClass : GeneratorBase<ClassDeclarationSyntax>
{
    public override void CreateGenerator(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclaration, BFNNexSyntaxReceiver syntaxReceiver)
    {
        var className = GetIdentifierName(classDeclaration);
        // Retrieve the properties of the class, needs to be filtered to DataContract
        var properties = propertyFinder.FilterProperties(context, classDeclaration.Members.OfType<PropertyDeclarationSyntax>());

        var serializerClassName = $"GeneratedSerializer{className}";
        var partialClass = SyntaxFactory.ClassDeclaration(serializerClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        NamespaceDeclarationSyntax normalNamespace = NamespaceCreator.CreateNamespace(classDeclaration, context, className);
        if (normalNamespace == null)
        {
            return;
        }

        string namespaceName = normalNamespace.Name.ToString();

        partialClass = CreateClassContent(context, classDeclaration, className, properties, partialClass);

        partialClass = AddInterfaces(partialClass, className);
        if (normalNamespace == null)
            return;
        var compilationUnit = SyntaxFactory.CompilationUnit()
                                              .AddMembers(normalNamespace.AddMembers(partialClass));

        var sourceText = compilationUnit.NormalizeWhitespace().ToFullString();

        context.AddSource($"{serializerClassName}_{namespaceName}.g.cs", sourceText);

    }
    PropertyAttributeFinder propertyFinder = new();
    

    private ClassDeclarationSyntax CreateClassContent(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclaration, string className, IEnumerable<PropertyDeclarationSyntax> properties, ClassDeclarationSyntax partialClass)
    {
        var inheritedProperties = propertyFinder.FilterInheritedProperties(classDeclaration, context);
        AddMethodsToTheClass(className, ref partialClass, properties, className, inheritedProperties);
        return partialClass;
    }
    private ConvertToYamlMethodFactory writerFactory;
    protected DeserializeMethodFactory DeserializeMethodFactory = new();

    private void AddMethodsToTheClass(string className, ref ClassDeclarationSyntax partialClass, IEnumerable<PropertyDeclarationSyntax> properties, string serializerClassName, IEnumerable<IPropertySymbol> inheritedProperties)
    {
        writerFactory = new();
        var writeToDictionaryString = writerFactory.ConvertToYamlTemplate(properties, serializerClassName, inheritedProperties);
        foreach (var privateProperty in writerFactory.PrivateProperties)
        {
            partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(privateProperty));
        }
        var deserializerMethodString = DeserializeMethodFactory.DeserializeMethodTemplate(properties, serializerClassName, inheritedProperties);
        var deserializerManyMethodString = DeserializeMethodFactory.DeserializeManyMethodTemplate(properties, serializerClassName, inheritedProperties);
        var deserializerFromYamlMappingNodeString = DeserializeMethodFactory.DeserializeFromYamlMappingNodeTemplate(properties, serializerClassName, inheritedProperties);
        var writeToDictionaryMethod = SyntaxFactory.ParseMemberDeclaration(writeToDictionaryString);
        var deserializeFromYamlMappingNodeMethod = SyntaxFactory.ParseMemberDeclaration(deserializerFromYamlMappingNodeString);
        var deserializeMethod = SyntaxFactory.ParseMemberDeclaration(deserializerMethodString);
        var identifierTagString = SerializedTypePropertyFactory.IdentifierTagTemplate(properties, serializerClassName, inheritedProperties);
        var identifierTypeString = IdentifierTypeFactory.IdentifierTagTemplate(properties, serializerClassName, inheritedProperties);
        partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(identifierTypeString));
        partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(identifierTagString));
        partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(deserializerManyMethodString));
        partialClass = partialClass.AddMembers(writeToDictionaryMethod);
        partialClass = partialClass.AddMembers(deserializeFromYamlMappingNodeMethod);
        partialClass = partialClass.AddMembers(deserializeMethod);
    }

}
