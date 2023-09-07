using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Roslyn;
using StrideSourceGenerator.Core.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace StrideSourceGenerator.Core.Methods;
internal class SerializeMethodFactory : IAppendableTemplate
{
    public List<string> PrivateProperties { get; set; } = new List<string>();
    public void Add(string name)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(name);
        StringBuilder sb = new StringBuilder();
        foreach (byte by in bytes)
        {
            sb.Append(by + ",");
        }
        PrivateProperties.Add($"private static readonly byte[] UTF8{name} = new byte[]{{{sb.ToString().Trim(',')}}};");
    }
    public void AppendTemplate(ClassInfo realClassInfo, StringBuilder builder)
    {
        string generic = realClassInfo.TypeName;
        string tag = $$"""
            if(context.IsRedirected || context.IsFirst)
            {
                emitter.Tag($"!{typeof({{realClassInfo.TypeName}})},{AssemblyName}");
                context.IsRedirected = false;
                context.IsFirst = false;
            }
            """;

        if (realClassInfo.Generics != null && realClassInfo.Generics.Parameters.Count > 0)
        {
            TypeParameterListSyntax typeParameterList = SyntaxFactory.TypeParameterList(realClassInfo.Generics.Parameters);
            realClassInfo.SerializerSyntax = realClassInfo.SerializerSyntax.WithTypeParameterList(typeParameterList);

            generic = $"{realClassInfo.TypeName}<{string.Join(", ", realClassInfo.Generics.Parameters)}>";
            tag = $$"""
                emitter.Tag($"!{typeof({{generic}})}".Replace('`','$'));
                """;
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
                if (!inheritedProperty.Type.IsAbstract && inheritedProperty.Type.IsReferenceType && !inheritedProperty.IsSealed && ((INamedTypeSymbol)inheritedProperty.Type).TypeArguments.Count() == 0)
                {
                    sb.Append($$"""
                     IYamlFormatter<{{inheritedProperty.Type.Name}}> {{propertyname}}formatter = context.Resolver.FindCompatibleFormatter(value.{{propertyname}},value.{{propertyname}}.GetType(),out bool is{{propertyname}}Redirected);
                     if({{propertyname}}formatter != null)
                     {
                        emitter.WriteString("{{propertyname}}", VYaml.Emitter.ScalarStyle.Plain);
                        context.IsRedirected = is{{propertyname}}Redirected;
                        {{propertyname}}formatter.Serialize(ref emitter, value.{{propertyname}},context);
                     }
                    """);
                }
                else
                {
                    sb.Append($"""
                     emitter.WriteString("{propertyname}", VYaml.Emitter.ScalarStyle.Plain);
                     context.Serialize(ref emitter, value.{propertyname});
                    """);
                }

            }
            Add(propertyname);

            //   else if (inheritedProperty.Type.TypeKind == TypeKind.Struct)
            //   {
            //       
            //       sb.Append($"new YamlScalarNode(nameof(objToSerialize.{propertyname})), new YamlScalarNode(objToSerialize.{propertyname}),");
            //   }
        }
        builder.Append($$"""
        public void Serialize(ref Utf8YamlEmitter emitter, {{generic}}? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
                return;
            }
            emitter.BeginMapping();
            {{tag}}
            {{sb}}
            emitter.EndMapping();
            
        }
        """);
    }
}