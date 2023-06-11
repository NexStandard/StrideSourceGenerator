using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.Core.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrideSourceGenerator.Core.GeneratorCreators;
internal class GeneratorYamlStruct : GeneratorBase<StructDeclarationSyntax>
{
    protected override string GeneratorClassPrefix { get; } = "GeneratedYamlSerializer";

    protected override ClassDeclarationSyntax CreateGenerator(GeneratorExecutionContext context, ClassDeclarationSyntax generatorClass, StructDeclarationSyntax contextClass)
    {
        var properties = PropertyFinder.FilterProperties(context, contextClass.Members.OfType<PropertyDeclarationSyntax>());
        generatorClass = CreateClassContent(context, contextClass, GetIdentifierName(contextClass), properties, generatorClass);

        return generatorClass;
    }
    private ClassDeclarationSyntax CreateClassContent(GeneratorExecutionContext context, StructDeclarationSyntax contextClass, string className, IEnumerable<PropertyDeclarationSyntax> properties, ClassDeclarationSyntax generatorClass)
    {
        AddMethodsToTheClass(className, ref generatorClass, properties, className);
        return generatorClass;
    }
    private void AddMethodsToTheClass(string className, ref ClassDeclarationSyntax partialClass, IEnumerable<PropertyDeclarationSyntax> properties, string serializerClassName)
    {
        writerFactory = new();
        var writeToDictionaryString = writerFactory.ConvertToYamlTemplate(properties, serializerClassName, Enumerable.Empty<IPropertySymbol>(), GeneratorClassPrefix);
        foreach (var privateProperty in writerFactory.PrivateProperties)
        {
            partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(privateProperty));
        }
        var deserializerMethodString = DeserializeMethodFactory.DeserializeMethodTemplate(properties, serializerClassName, Enumerable.Empty<IPropertySymbol>());
        var deserializerManyMethodString = DeserializeMethodFactory.DeserializeManyMethodTemplate(properties, serializerClassName, Enumerable.Empty<IPropertySymbol>());
        var deserializerFromYamlMappingNodeString = DeserializeMethodFactory.DeserializeFromYamlMappingNodeTemplate(properties, serializerClassName, Enumerable.Empty<IPropertySymbol>(),GeneratorClassPrefix);
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
