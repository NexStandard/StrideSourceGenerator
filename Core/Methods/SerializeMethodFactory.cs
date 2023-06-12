using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace StrideSourceGenerator.Core.Methods;
internal class SerializeMethodFactory
{
    public List<string> PrivateProperties { get; set; } = new List<string>();
    public void Add(string name)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(name);
        StringBuilder sb = new StringBuilder();
        foreach (var by in bytes)
        {
            sb.Append(by+",");
        }
        PrivateProperties.Add($"private static readonly byte[] UTF8{name} = new byte[]{{{sb.ToString().Trim(',')}}};");
    }
    public string ConvertToYamlTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className, IEnumerable<IPropertySymbol> symbols,string serializerClassNamePrefix)
    {

        StringBuilder sb = new StringBuilder();
        foreach (var inheritedProperty in symbols)
        {
            var propertyname = inheritedProperty.Name;
            var type = inheritedProperty.Type.Name;

            sb.Append($"""
                     emitter.WriteString("{propertyname}", ScalarStyle.Plain);
                     context.Serialize(ref emitter, value.{propertyname});
                    """);
            Add(propertyname);

            //   else if (inheritedProperty.Type.TypeKind == TypeKind.Struct)
            //   {
            //       
            //       sb.Append($"new YamlScalarNode(nameof(objToSerialize.{propertyname})), new YamlScalarNode(objToSerialize.{propertyname}),");
            //   }
        }

        return $$"""
        public void Serialize(ref Utf8YamlEmitter emitter, global::StrideSourceGened.{{className}}? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
                return;
            }
            emitter.BeginMapping();
            {{sb}}
            emitter.EndMapping();
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
            return $"mappedResult.Add({name}, objToSerialize.{name});";
        if (type == "Guid")
            return $"mappedResult.Add({name}, objToSerialize.{name}.ToString())";
        return $"mappedResult.Add({name}, objToSerialize.{name}.ToString());";
    }
}