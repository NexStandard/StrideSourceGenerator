using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using StrideSourceGenerator.Core.Methods;
using StrideSourceGenerator.Core.Roslyn;
using StrideSourceGenerator.Core.Properties;

namespace StrideSourceGenerator.Core.GeneratorCreators;
internal class GeneratorYamlClass : GeneratorBase<ClassDeclarationSyntax>
{
    protected override ClassDeclarationSyntax CreateGenerator(ClassInfo<ClassDeclarationSyntax> classInfo)
    {
        PropertyAttributeFinder finder = new PropertyAttributeFinder();
        classInfo.SerializerSyntax = AddMember(classInfo.SerializerSyntax, RegistrationInResolver(classInfo.SerializerName));
        var symbol = classInfo.Symbol;
        var properties = finder.FilterBasePropertiesRecursive(ref symbol);
        classInfo.SerializerSyntax =  AddMethodsToClass(classInfo.SerializerSyntax, properties, classInfo.TypeName);
        return classInfo.SerializerSyntax;
    }

    private SerializeMethodFactory writerFactory;
    protected DeserializeMethodFactory DeserializeMethodFactory = new();

    protected override string GeneratorClassPrefix { get; } = "GeneratedYamlSerializer";

    protected ClassDeclarationSyntax AddMethodsToClass(ClassDeclarationSyntax classContext, IEnumerable<IPropertySymbol> properties, string typeName)
    {

        writerFactory = new();
        var res = writerFactory.ConvertToYamlTemplate(Enumerable.Empty<PropertyDeclarationSyntax>(), typeName,properties,GeneratorClassPrefix);
        foreach (var privateProperty in writerFactory.PrivateProperties)
        {
            classContext = AddMember(classContext,privateProperty);
        }
        classContext = AddMember(classContext, res);
        var identifierTypeString = IdentifierTypeFactory.IdentifierTagTemplate(typeName);
        classContext = classContext.AddMembers(SyntaxFactory.ParseMemberDeclaration(identifierTypeString));
        classContext = classContext.AddMembers(SyntaxFactory.ParseMemberDeclaration($"public global::StrideSourceGened.{typeName}? Deserialize(ref YamlParser parser, YamlDeserializationContext context){{return null;}}"));
      /*  classContext = AddMember(classContext, SerializedTypePropertyFactory.IdentifierTagTemplate(serializerClassName));
        classContext = AddMember(classContext, writerFactory.ConvertToYamlTemplate(properties, serializerClassName, properties, GeneratorClassPrefix));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeFromYamlMappingNodeTemplate(properties, serializerClassName, properties, GeneratorClassPrefix));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeManyMethodTemplate(properties, serializerClassName, properties));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeMethodTemplate(properties, serializerClassName, properties));
        */
        return classContext;
    }
}
