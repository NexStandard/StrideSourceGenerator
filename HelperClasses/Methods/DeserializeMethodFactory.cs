using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace StrideSourceGenerator.HelperClasses.Methods;
internal class DeserializeMethodFactory
{
    public string DeserializeManyMethodTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols)
    {
        return $$"""
        public IEnumerable<{{className}}> DeserializeMany(TextReader reader)
        {
            YamlStream stream = new YamlStream();
            stream.Load(reader);
            List<YamlDocument> documents = stream.Documents;
            if(documents is null)
                yield break;
            if(documents.Count == 0)
                yield break;

            for(int i = 0; i< documents.Count;i++)
            {
                 yield return Deserialize((YamlMappingNode)documents[i].RootNode);
            }
        }
        """;

    }
    public string DeserializeMethodTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols)
    {
        return $$"""
        public {{className}} Deserialize(TextReader reader)
        {
            YamlStream stream = new YamlStream();
            stream.Load(reader);
            List<YamlDocument> documents = stream.Documents;
            if(documents is null)
                return null;
            if(documents.Count == 0)
                return null;
            return Deserialize((YamlMappingNode)documents[0].RootNode);
        }
        """;

    }
    public string DeserializeFromYamlMappingNodeTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var inheritedProperty in symbols)
        {
            var propertyname = inheritedProperty.Name;
            var type = inheritedProperty.Type.Name;
            if (SimpleTypes.Contains(type))
            {
                sb.Append(CreateSimpleType(propertyname, type));
            }
            else if (inheritedProperty.Type.TypeKind == TypeKind.Class)
            {

                sb.Append($"{propertyname} = new GeneratedSerializer{type}().DeserializeFromYamlNode(dictionaryDocument[\"{propertyname}\"]),");
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
                sb.Append($"{propertyName} = new GeneratedSerializer{type}().Deserialize((YamlMappingNode)dictionaryDocument[\"{propertyName}\"]),");
            }
        }
        return $$"""
        public {{className}} Deserialize(YamlMappingNode node)
        {
            if(node is null)
                return null;

            var dictionaryDocument = node.Children;
            if(dictionaryDocument.Count == 0)
                return null;
            {{className}} result = new {{className}}()
            {
                {{sb.ToString().TrimEnd(',')}}
            };
            return result;
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
        if (type == "int" || type == "Int32")
            return $"{name} =  Int32.Parse(((YamlScalarNode)dictionaryDocument[\"{name}\"]).Value),";
        if (type == "string" || type == "String")
            return $"{name} =  dictionaryDocument[\"{name}\"].ToString(),";
        // TODO: this is wrong, the other types need to get implemented
        return $"{name} =  Int32.Parse(((YamlScalarNode)dictionaryDocument[\"{name}\"]).Value),";
    }

}
