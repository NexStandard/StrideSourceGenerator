using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using StrideSourceGenerator.Core.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Methods.RegisterTemplates;

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
            TypeParameterListSyntax typeParameterList = SyntaxFactory.TypeParameterList(classInfo.Generics.Parameters);

            classInfo.SerializerSyntax = classInfo.SerializerSyntax.WithTypeParameterList(typeParameterList);

            generic = $"{classInfo.TypeName}<{string.Join(", ", classInfo.Generics.Parameters)}>";
        }
        foreach (IPropertySymbol property in properties)
        {
            defaultValues.Append("var temp").Append(property.Name).Append($"= default({property.Type});");
            objectCreation.Append(property.Name + "=" + "temp" + property.Name + ",");
        }
        string finishedMethod = $$"""
         public {{generic}}? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
                {{MapPropertiesToSwitch(map)}}
                default:
                    parser.Read();
                    parser.SkipCurrentNode();
                    continue;
                 }
             }

             parser.ReadWithVerify(ParseEventType.MappingEnd);
             return new {{generic}}
             {
                 {{objectCreation.ToString().Trim(',')}}
             };
         }
         """;
        return SyntaxFactory.ParseMemberDeclaration(finishedMethod);
    }
    public StringBuilder MapPropertiesToSwitch(Dictionary<int, List<IPropertySymbol>> properties)
    {
        StringBuilder switchFinder = new StringBuilder();
        AppendWhileLoopStart(switchFinder);
        IPropertyAppendableTemplate arrayAppender;
        IPropertyAppendableTemplate propertyAppender;
        foreach (KeyValuePair<int, List<IPropertySymbol>> prop in properties)
        {
            AppendSwitchCase(switchFinder, prop);
            bool isFirstime = true;
            foreach (IPropertySymbol propert in prop.Value)
            {
                arrayAppender = new DeserializeArrayAppenderTemplate() { IsFirstTime = isFirstime };
                propertyAppender = new DeserializePropertyAppenderTemplate() { IsFirstTime = isFirstime, };
                if (isFirstime)
                {
                    if (propert.Type.TypeKind == TypeKind.Array)
                    {
                        arrayAppender.AppendTemplate(propert, switchFinder);
                    }
                    else
                    {
                        propertyAppender.AppendTemplate(propert, switchFinder);
                    }
                    isFirstime = false;
                }
                else
                {
                    if (propert.Type.TypeKind == TypeKind.Array)
                    {
                        arrayAppender.AppendTemplate(propert, switchFinder);
                    }
                    else
                    {
                        propertyAppender.AppendTemplate(propert, switchFinder);
                    }
                }
            }
            AppendElseSkip(switchFinder);
        }
        AppendDefaultCase(switchFinder);
        return switchFinder;
    }

    private static void AppendDefaultCase(StringBuilder switchFinder)
    {
        switchFinder.Append($$"""

        """);
    }

    static KeyValuePair<int, List<IPropertySymbol>> AppendSwitchCase(StringBuilder switchFinder, KeyValuePair<int, List<IPropertySymbol>> prop)
    {
        switchFinder.Append("case " + prop.Key + ":");
        return prop;
    }

    static void AppendWhileLoopStart(StringBuilder switchFinder)
    {
        switchFinder.Append($$"""

                """);
    }

    static StringBuilder AppendElseSkip(StringBuilder switchFinder)
    {
        return switchFinder.Append("""
                else
                {
                    parser.Read();
                    parser.SkipCurrentNode();
                }
                continue;
                """);
    }
}
