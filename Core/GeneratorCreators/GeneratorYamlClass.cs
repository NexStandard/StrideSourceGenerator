using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using StrideSourceGenerator.Core.Methods;
using StrideSourceGenerator.Core.Roslyn;
using StrideSourceGenerator.Core.Properties;
using StrideSourceGenerator.Core.Templates;
using System.Text;

namespace StrideSourceGenerator.Core.GeneratorCreators;
internal class GeneratorYamlClass : GeneratorBase
{
    protected override ClassDeclarationSyntax CreateGenerator(ClassInfo classInfo)
    {
        IEnumerable<IPropertySymbol> allProperties = classInfo.AvailableProperties;
        classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(RegisterMethodFactory.GetRegisterMethod(classInfo));
        classInfo.SerializerSyntax = AddMethodsToClass(allProperties, classInfo);
        return classInfo.SerializerSyntax;
    }

    protected override string GeneratorClassPrefix { get; } = "GeneratedYamlSerializer";

    protected ClassDeclarationSyntax AddMethodsToClass(IEnumerable<IPropertySymbol> properties, ClassInfo classInfo)
    {
        TypeParameterListSyntax generics = classInfo.Generics;
        if (generics != null)
        {
            int count = generics.Parameters.Count;
            string x = "<" + new string(',', count - 1) + ">";

            classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(TypeTemplate.GetTemplate(classInfo));
            classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(new AssemblyTemplateProvider().GetTemplate(classInfo));
        }
        else
        {
            classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(TypeTemplate.GetTemplate(classInfo));
            classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddMembers(new AssemblyTemplateProvider().GetTemplate(classInfo));
        }
        writerFactory = new();

        foreach (string privateProperty in writerFactory.PrivateProperties)
        {
            classInfo.SerializerSyntax = AddMember(classInfo.SerializerSyntax, privateProperty);
        }
        StringBuilder builder = new StringBuilder();
        writerFactory.AppendTemplate(classInfo, builder);
        classInfo.SerializerSyntax = AddMember(classInfo.SerializerSyntax, builder.ToString());

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
