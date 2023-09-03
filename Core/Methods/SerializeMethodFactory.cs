using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.Core.Roslyn;
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
        foreach (byte by in bytes)
        {
            sb.Append(by+",");
        }
        PrivateProperties.Add($"private static readonly byte[] UTF8{name} = new byte[]{{{sb.ToString().Trim(',')}}};");
    }
    public string ConvertToYamlTemplate(ClassInfo realClassInfo)
    {
        string generic = realClassInfo.TypeName;
        if (realClassInfo.Generics != null && realClassInfo.Generics.Parameters.Count > 0)
        {
            var typeParameterList = SyntaxFactory.TypeParameterList(realClassInfo.Generics.Parameters);

            realClassInfo.SerializerSyntax = realClassInfo.SerializerSyntax.WithTypeParameterList(typeParameterList);

            generic = $"{realClassInfo.TypeName}<{string.Join(", ", realClassInfo.Generics.Parameters)}>";
        }

        StringBuilder sb = new StringBuilder();
        foreach (IPropertySymbol inheritedProperty in realClassInfo.AvailableProperties)
        {
            string propertyname = inheritedProperty.Name;

            if (inheritedProperty.Type.TypeKind == TypeKind.Array)
            {
                IArrayTypeSymbol arrayType = (IArrayTypeSymbol)inheritedProperty.Type;

                if (arrayType.ElementType.SpecialType == SpecialType.System_Byte)
                {
                    sb.Append($"""
                     emitter.WriteString("{propertyname}", VYaml.Emitter.ScalarStyle.Plain);
                     context.SerializeByteArray(ref emitter, value.{propertyname});
                    """);
                }
                else
                {
                    sb.Append($"""
                     emitter.WriteString("{propertyname}", VYaml.Emitter.ScalarStyle.Plain);
                     context.SerializeArray(ref emitter, value.{propertyname});
                    """);
                }

            }
            else
            {
                sb.Append($"""
                     emitter.WriteString("{propertyname}", VYaml.Emitter.ScalarStyle.Plain);
                     context.Serialize(ref emitter, value.{propertyname});
                    """);
            }
            Add(propertyname);

            //   else if (inheritedProperty.Type.TypeKind == TypeKind.Struct)
            //   {
            //       
            //       sb.Append($"new YamlScalarNode(nameof(objToSerialize.{propertyname})), new YamlScalarNode(objToSerialize.{propertyname}),");
            //   }
        }

            return $$"""
        public void Serialize(ref Utf8YamlEmitter emitter, {{generic}}? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
                return;
            }
            emitter.BeginMapping();
            emitter.Tag($"!{typeof({{generic}})}");
            {{sb}}
            emitter.EndMapping();
            
        }
        """;

    }
}