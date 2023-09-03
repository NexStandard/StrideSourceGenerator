using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using StrideSourceGenerator.Core.Methods;
using StrideSourceGenerator.Core.Roslyn;
using StrideSourceGenerator.Core.Properties;

namespace StrideSourceGenerator.Core.GeneratorCreators;
internal class GeneratorYamlClass : GeneratorBase
{
    protected override ClassDeclarationSyntax CreateGenerator(ClassInfo classInfo)
    {
        IEnumerable<IPropertySymbol> allProperties = classInfo.AvailableProperties;
        classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(RegisterMethodFactory.GetRegisterMethod(classInfo));
        classInfo.SerializerSyntax =  AddMethodsToClass(allProperties,classInfo);
        return classInfo.SerializerSyntax;
    }

    protected override string GeneratorClassPrefix { get; } = "GeneratedYamlSerializer";

    protected ClassDeclarationSyntax AddMethodsToClass(IEnumerable<IPropertySymbol> properties, ClassInfo classInfo)
    {

        writerFactory = new();
        
        string res = writerFactory.ConvertToYamlTemplate(classInfo);
        foreach (string privateProperty in writerFactory.PrivateProperties)
        {
            classInfo.SerializerSyntax = AddMember(classInfo.SerializerSyntax,privateProperty);
        }
        
        classInfo.SerializerSyntax = AddMember(classInfo.SerializerSyntax, res);
        var generics = classInfo.Generics;
        if(generics != null)
        {
            var count = generics.Parameters.Count;
            var x = "<"+new string(',', count-1)+">";
            classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(TypeTemplate.GetTemplate(classInfo.TypeName+x));
        }
        else
        {
            classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(TypeTemplate.GetTemplate(classInfo.TypeName));
        }

        classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(DeserializeMethodFactory.GetMethod(classInfo));
        
      /*  classContext = AddMember(classContext, SerializedTypePropertyFactory.IdentifierTagTemplate(serializerClassName));
        classContext = AddMember(classContext, writerFactory.ConvertToYamlTemplate(properties, serializerClassName, properties, GeneratorClassPrefix));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeFromYamlMappingNodeTemplate(properties, serializerClassName, properties, GeneratorClassPrefix));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeManyMethodTemplate(properties, serializerClassName, properties));
        classContext = AddMember(classContext, DeserializeMethodFactory.DeserializeMethodTemplate(properties, serializerClassName, properties));
        */
        return classInfo.SerializerSyntax;
    }
}
