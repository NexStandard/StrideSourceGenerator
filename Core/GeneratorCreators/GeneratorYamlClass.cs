using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using StrideSourceGenerator.Core.Methods;

namespace StrideSourceGenerator.Core.GeneratorCreators;
internal class GeneratorYamlClass : GeneratorBase<ClassDeclarationSyntax>
{
    protected override ClassDeclarationSyntax CreateGenerator(GeneratorExecutionContext context, ClassDeclarationSyntax generatorClass, ClassDeclarationSyntax contextClass)
    {
        var properties = PropertyFinder.FilterProperties(context, contextClass.Members.OfType<PropertyDeclarationSyntax>());
        generatorClass = CreateClassContent(context, contextClass, GetIdentifierName(contextClass), properties, generatorClass);
        return generatorClass;
    }


    private ClassDeclarationSyntax CreateClassContent(GeneratorExecutionContext context, ClassDeclarationSyntax contextClass, string className, IEnumerable<PropertyDeclarationSyntax> properties, ClassDeclarationSyntax generatorClass)
    {
        var inheritedProperties = PropertyFinder.FilterInheritedProperties(contextClass, context);
        AddMethodsToTheClass(className, ref generatorClass, properties, className, inheritedProperties);
        return generatorClass;
    }
    private ConvertToYamlMethodFactory writerFactory;
    protected DeserializeMethodFactory DeserializeMethodFactory = new();

    protected override string GeneratorClassPrefix { get; } = "GeneratedYamlSerializer";

    private void AddMethodsToTheClass(string className, ref ClassDeclarationSyntax partialClass, IEnumerable<PropertyDeclarationSyntax> properties, string serializerClassName, IEnumerable<IPropertySymbol> inheritedProperties)
    {
        writerFactory = new();
        var writeToDictionaryString = writerFactory.ConvertToYamlTemplate(properties, serializerClassName, inheritedProperties, GeneratorClassPrefix);
        foreach (var privateProperty in writerFactory.PrivateProperties)
        {
            partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(privateProperty));
        }
        var deserializerMethodString = DeserializeMethodFactory.DeserializeMethodTemplate(properties, serializerClassName, inheritedProperties);
        var deserializerManyMethodString = DeserializeMethodFactory.DeserializeManyMethodTemplate(properties, serializerClassName, inheritedProperties);
        var deserializerFromYamlMappingNodeString = DeserializeMethodFactory.DeserializeFromYamlMappingNodeTemplate(properties, serializerClassName, inheritedProperties,GeneratorClassPrefix);
        var writeToDictionaryMethod = SyntaxFactory.ParseMemberDeclaration(writeToDictionaryString);
        var deserializeFromYamlMappingNodeMethod = SyntaxFactory.ParseMemberDeclaration(deserializerFromYamlMappingNodeString);
        var deserializeMethod = SyntaxFactory.ParseMemberDeclaration(deserializerMethodString);
        var identifierTagString = SerializedTypePropertyFactory.IdentifierTagTemplate(serializerClassName);
        var identifierTypeString = IdentifierTypeFactory.IdentifierTagTemplate(serializerClassName);
        partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(identifierTypeString));
        partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(identifierTagString));
        partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(deserializerManyMethodString));
        partialClass = partialClass.AddMembers(writeToDictionaryMethod);
        partialClass = partialClass.AddMembers(deserializeFromYamlMappingNodeMethod);
        partialClass = partialClass.AddMembers(deserializeMethod);
    }

}
