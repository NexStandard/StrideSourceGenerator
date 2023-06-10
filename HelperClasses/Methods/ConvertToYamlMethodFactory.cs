using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace StrideSourceGenerator.HelperClasses.Methods;
internal class ConvertToYamlMethodFactory
{
    public List<string> PrivateProperties { get; set; } = new List<string>();
    public void Add(string name)
    {
        // TODO : static until SerializerRegistry works
        PrivateProperties.Add($"private static YamlScalarNode {name} = new YamlScalarNode(\"{name}\");");
    }
    public string ConvertToYamlTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols)
    {

        StringBuilder sb = new StringBuilder();
        foreach (var inheritedProperty in symbols)
        {
            var propertyname = inheritedProperty.Name;
            var type = inheritedProperty.Type.Name;
            if (SimpleTypes.Contains(type))
            {
                sb.Append(CreateSimpleType(propertyname, type));
                Add(propertyname);
            }
            else if (inheritedProperty.Type.TypeKind == TypeKind.Class)
            {

                sb.Append($"mappedResult.Add({propertyname}, new GeneratedSerializer{type}().ConvertToYaml(objToSerialize.{propertyname}));");
                Add(propertyname);
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
                Add(propertyName);
            }


            else if (property.Type is IdentifierNameSyntax classIdentifier)
            {
                sb.Append($"mappedResult.Add({propertyName}, new GeneratedSerializer{type}().ConvertToYaml(objToSerialize.{propertyName}));");
                Add(propertyName);
            }
        }

        return $$"""
        public YamlMappingNode ConvertToYaml({{className}} objToSerialize)
        {
        if(objToSerialize is not null) { 
        var mappedResult = new YamlMappingNode()
        {
            Tag = "!{{className}}"
        };

        {{sb}}

        return mappedResult;
        }
        return new YamlMappingNode();
        }
        """;

    }

    private List<string> SimpleTypes = new()
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
        if (type == "string" || type == "String")
        {
            return $"mappedResult.Add({name}, objToSerialize.{name});";
        }
        return $"mappedResult.Add({name}, objToSerialize.{name}.ToString());";
    }
}