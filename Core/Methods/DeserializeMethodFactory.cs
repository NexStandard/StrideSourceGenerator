using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using StrideSourceGenerator.Core.Roslyn;
using Microsoft.CodeAnalysis.CSharp;

namespace StrideSourceGenerator.Core.Methods;
internal class DeserializeMethodFactory : IDeserializeMethodFactory
{
    public MemberDeclarationSyntax GetMethod(ClassInfo classInfo)
    {
        StringBuilder defaultValues = new StringBuilder();
        IEnumerable<IPropertySymbol> properties = classInfo.AvailableProperties;
        Dictionary<int, List<IPropertySymbol>> map = this.MapPropertiesToLength(properties);
        StringBuilder objectCreation = new StringBuilder();
        string generic = classInfo.TypeName;
        if (classInfo.Generics != null && classInfo.Generics.Parameters.Count > 0)
        {
            var typeParameterList = SyntaxFactory.TypeParameterList(classInfo.Generics.Parameters);

            classInfo.SerializerSyntax = classInfo.SerializerSyntax.WithTypeParameterList(typeParameterList);

            generic = $"{classInfo.TypeName}<{string.Join(", ", classInfo.Generics.Parameters)}>";
        }
        foreach (IPropertySymbol property in properties)
        {
            defaultValues.Append("var temp").Append(property.Name).Append($"= default({property.Type});");
            objectCreation.Append(property.Name + "=" + "temp" + property.Name + ",");
        }
        var x = $$"""
             public {{generic}}? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
             {
                 if (parser.IsNullScalar())
                 {
                     parser.Read();
                     return default;
                 }
                 parser.ReadWithVerify(ParseEventType.MappingStart);
                 {{defaultValues}}
         {{MapPropertiesToSwitch(map)}}

         parser.ReadWithVerify(ParseEventType.MappingEnd);
         return new {{generic}}
         {
             {{objectCreation.ToString().Trim(',')}}
         };
         }
         """;
        return SyntaxFactory.ParseMemberDeclaration(x);
    }
    public StringBuilder MapPropertiesToSwitch(Dictionary<int, List<IPropertySymbol>> properties)
    {
        StringBuilder switchFinder = new StringBuilder();
        switchFinder.Append($$"""
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
                """);
        foreach (KeyValuePair<int, List<IPropertySymbol>> prop in properties)
        {

            switchFinder.Append("case " + prop.Key + ":");
            int counter = 0;
            foreach (IPropertySymbol propert in prop.Value)
            {
                if (counter == 0)
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
        switchFinder.Append($$"""
        default:
                    parser.Read();
                    parser.SkipCurrentNode();
                    continue;
            }
        }
        """);
        return switchFinder;
    }
}
