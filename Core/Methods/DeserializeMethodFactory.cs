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
        foreach (IPropertySymbol property in symbols)
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
        foreach (KeyValuePair<int, List<IPropertySymbol>> prop in map)
        {
            switchFinder.Append("case " + prop.Key+":");
            int counter = 0;
            foreach(IPropertySymbol propert in prop.Value)
            {
                if(counter == 0)
                {
                    if (propert.Type.TypeKind == TypeKind.Array)
                    {
                        IArrayTypeSymbol arrayType = (IArrayTypeSymbol)propert.Type;

                        if (arrayType.ElementType.SpecialType == SpecialType.System_Byte)
                        {
                            switchFinder.Append($$"""
                            if (key.SequenceEqual({{"UTF8" + propert.Name}}))
                            {
                                parser.Read();
                                temp{{propert.Name}} = context.DeserializeByteArray(ref parser);
                            }
                            """);
                        }
                        else
                        {
                            switchFinder.Append($$"""
                            if (key.SequenceEqual({{"UTF8" + propert.Name}}))
                            {
                                parser.Read();
                                temp{{propert.Name}} = context.DeserializeArray<{{arrayType.ElementType}}>(ref parser);
                            }
                            """);
                        }

                    }
                    else
                    {
                        switchFinder.Append($$"""
                        if (key.SequenceEqual({{"UTF8" + propert.Name}}))
                        {
                            parser.Read();
                            temp{{propert.Name}} = context.DeserializeWithAlias<{{propert.Type}}>(ref parser);
                        }
                        """);
                    }
                    counter = 1;
                }
                else
                {
                    if (propert.Type.TypeKind == TypeKind.Array)
                    {

                        switchFinder.Append($$"""
                            else if (key.SequenceEqual({{"UTF8" + propert.Name}}))
                            {
                                parser.Read();
                                temp{{propert.Name}} = context.DeserializeWithAlias<{{propert.Type}}>(ref parser);
                            }
                            """);
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
}
