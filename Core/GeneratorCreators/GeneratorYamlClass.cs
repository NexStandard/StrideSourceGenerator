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
        var compilation = classInfo.ExecutionContext.Compilation.GetSemanticModel(classInfo.TypeSyntax.SyntaxTree);
        var classSymbol = compilation.GetDeclaredSymbol(classInfo.TypeSyntax);
        IEnumerable<IPropertySymbol> allProperties = PropertyAttributeFinder.FilterBasePropertiesRecursive(ref classSymbol);
        classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(RegisterMethodFactory.GetRegisterMethod(classInfo)); ;
        classInfo.SerializerSyntax =  AddMethodsToClass(classInfo.SerializerSyntax, allProperties, classInfo.TypeName);
        return classInfo.SerializerSyntax;
    }

    protected override string GeneratorClassPrefix { get; } = "GeneratedYamlSerializer";

    protected ClassDeclarationSyntax AddMethodsToClass(ClassDeclarationSyntax classContext, IEnumerable<IPropertySymbol> properties, string typeName)
    {

        writerFactory = new();
        string res = writerFactory.ConvertToYamlTemplate(Enumerable.Empty<PropertyDeclarationSyntax>(), typeName,properties,GeneratorClassPrefix);
        foreach (string privateProperty in writerFactory.PrivateProperties)
        {
            classContext = AddMember(classContext,privateProperty);
        }
        
        classContext = AddMember(classContext, res);
        classContext = classContext.AddMembers(TypeTemplate.GetTemplate(typeName));
        classContext = classContext.AddMembers(SyntaxFactory.ParseMemberDeclaration(DeserializeMethodFactory.DeserializeMethodTemplate(typeName, properties)));
      /*  classContext = AddMember(classContext, SerializedTypePropertyFactory.IdentifierTagTemplate(serializerClassName));
        classContext = AddMember(classContext, writerFactory.ConvertToYamlTemplate(properties, serializerClassName, properties, GeneratorClassPrefix));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeFromYamlMappingNodeTemplate(properties, serializerClassName, properties, GeneratorClassPrefix));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeManyMethodTemplate(properties, serializerClassName, properties));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeMethodTemplate(properties, serializerClassName, properties));
        */
        return classContext;
    }
}
