using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace StrideSourceGenerator;
internal class WriterFactory
{
    public string Test(IEnumerable<PropertyDeclarationSyntax> properties, string className, List<IPropertySymbol> symbols)
    {
        return WriteToDictionaryTemplate(properties, className,symbols);

    }
    public string WriteToDictionaryTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className,IEnumerable<IPropertySymbol> symbols)
    {
 
        StringBuilder sb = new StringBuilder();
        foreach(var inheritedProperty in symbols)
        {
            var propertyname = inheritedProperty.Name;
            var type = inheritedProperty.Type.Name;
            if (SimpleTypes.Contains(type))
            {
                sb.Append(CreateSimpleType(propertyname, type));
            }
            else if (inheritedProperty.Type.TypeKind == TypeKind.Class)
            {
                
                sb.Append($"new YamlScalarNode(nameof(objToSerialize.{propertyname})), GeneratedSerializer{type}.ConvertToYaml(objToSerialize.{propertyname}),");
            }
         //   else if (inheritedProperty.Type.TypeKind == TypeKind.Struct)
         //   {
         //       
         //       sb.Append($"new YamlScalarNode(nameof(objToSerialize.{propertyname})), new YamlScalarNode(objToSerialize.{propertyname}),");
         //   }
        }
        
        foreach (var property in properties)
        {
            var propertyName = property.Identifier.Text;
            var type = property.Type.ToString();

            if (SimpleTypes.Contains(type))
            {
                sb.Append(CreateSimpleType(propertyName, type));
            }
            

            else if (property.Type is IdentifierNameSyntax classIdentifier)
            {
                sb.Append($"new YamlScalarNode(nameof(objToSerialize.{propertyName})), GeneratedSerializer{type}.ConvertToYaml(objToSerialize.{propertyName}),");
            }
        }

        return $$"""
        public static YamlMappingNode ConvertToYaml({{className}} objToSerialize)
        {
        if(objToSerialize is not null) { 
        var mappedResult = new YamlMappingNode(
            new YamlScalarNode("$Class"), new YamlScalarNode(nameof({{className}})),
            {{sb}}
            new YamlScalarNode("$Namespace"), new YamlScalarNode(typeof({{className}}).Namespace)
        );
        return mappedResult;
        }
        return new YamlMappingNode(
            new YamlScalarNode("$Class"), new YamlScalarNode(nameof({{className}})),
            new YamlScalarNode("$Namespace"), new YamlScalarNode(typeof({{className}}).Namespace),
            new YamlScalarNode("$IsNull"), new YamlScalarNode("true")
        );
        }
        """;
        
    }

    private List<string> SimpleTypes = new ()
    {
        "int",
        "Int32",
        "string",
        "String",
        "float",
        "double",
        "long",
        "UInt64",
        "Int64",
        "byte",
        "Byte"
    };
    private string CreateSimpleType(string name, string type)
    {
        return $"new YamlScalarNode(nameof(objToSerialize.{name})), new YamlScalarNode(objToSerialize.{name}.ToString()),";
    }
}
