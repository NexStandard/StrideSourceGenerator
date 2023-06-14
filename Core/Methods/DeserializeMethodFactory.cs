using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace StrideSourceGenerator.Core.Methods;
internal class DeserializeMethodFactory
{
    public string DeserializeMethodTemplate(string className, IEnumerable<IPropertySymbol> symbols)
    {
        StringBuilder defaultValues = new StringBuilder();
        Dictionary<int, List<IPropertySymbol>> map = new ();
        StringBuilder objectCreation = new StringBuilder();
        foreach (var property in symbols)
        {
            int propertyLength = property.Name.Length;
            defaultValues.Append("var temp").Append(property.Name).Append($"= default({property.Type});");
            objectCreation.Append(property.Name+"="+"temp"+ property.Name+",");
            if(!map.ContainsKey(propertyLength)) 
            {
                map.Add(propertyLength, new() { property});
            }
            else
            {
                map[propertyLength].Add(property);
            }
        }
        StringBuilder switchFinder = new StringBuilder();
        foreach (var prop in map)
        {
            switchFinder.Append("case " + prop.Key+":");
            int counter = 0;
            foreach(var propert in prop.Value)
            {
                if(counter == 0)
                {
                    switchFinder.Append($$"""
                        if (key.SequenceEqual({{"UTF8" + propert.Name}}))
                        {
                            parser.Read();
                            temp{{propert.Name}} = context.DeserializeWithAlias<{{propert.Type}}>(ref parser);
                        }
                        """);
                    counter = 1;
                }
                else
                {
                    switchFinder.Append($$"""
                        else if (key.SequenceEqual({{"UTF8" + propert.Name}}))
                        {
                            parser.Read();
                            temp{{propert.Name}} = context.DeserializeWithAlias<{{propert.Type}}>(ref parser);
                        }
                        """);
                }
                
            }
            switchFinder.Append("""
                else
                {
                    parser.Read();
                    parser.SkipCurrentNode();
                }
                continue;
                """);
        }
        return $$"""
             public {{className}}? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
             {
                 if (parser.IsNullScalar())
                 {
                     parser.Read();
                     return default;
                 }
                 parser.ReadWithVerify(ParseEventType.MappingStart);
                 {{defaultValues}}
         while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
         {
             if (parser.CurrentEventType != ParseEventType.Scalar)
             {
                 throw new YamlSerializerException(parser.CurrentMark, "Custom type deserialization supports only string key");
             }

             if (!parser.TryGetScalarAsSpan(out var key))
             {
                 throw new YamlSerializerException(parser.CurrentMark, "Custom type deserialization supports only string key");
             }

             switch (key.Length)
             {
             {{switchFinder}}
                 default:
                     parser.Read();
                     parser.SkipCurrentNode();
                     continue;
             }
         }
         parser.ReadWithVerify(ParseEventType.MappingEnd);
         return new {{className}}
         {
             {{objectCreation.ToString().Trim(',')}}
         };
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
