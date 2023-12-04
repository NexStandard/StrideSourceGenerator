﻿using Microsoft.CodeAnalysis;
using StrideSourceGenerator.NexAPI;
using StrideSourceGenerator.Templates;
using StrideSourceGenerator.Templates.Registration;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.NexIncremental
{
    internal class SourceCreator
    {
        private static readonly ITemplate thisRegister = new ThisRegister();
        private static readonly ITemplate abstractRegister = new AbstractRegister();
        private static readonly ITemplate interfaceRegister = new InterfaceRegister();
        private static readonly ITemplate serializerEmitter = new SerializeEmitter();
        internal string Create(SourceProductionContext ctx, ClassInfo info)
        {
            string ns = (info.NameSpace != null ? "namespace " + info.NameSpace + ";" : "");
            StringBuilder tempVariables = new StringBuilder();
            foreach (var member in info.MemberSymbols)
            {
                tempVariables.AppendLine($"var temp_{member.Name} = default({member.Type});");
            }
            string Template = @$"// <auto-generated/>
//  This code was generated by Strides YamlSerializer.
//  Do not edit this file.

#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using VYaml;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;
using Stride.Core;
{ns}
[System.CodeDom.Compiler.GeneratedCode(""StrideYaml"",""1.0.0.0"")]
public class {info.GeneratorName} : IYamlFormatter<{info.Name}>
{{
    string AssemblyName {{ get; }} = typeof({info.Name}).Assembly.GetName().Name;
    string IdentifierTag {{ get; }} = typeof({info.Name}).Name;
    Type IdentifierType {{ get; }} = typeof({info.Name});

    {info.Accessor} void Register()
    {{
        {thisRegister.Create(info)}
        {abstractRegister.Create(info)}
        {interfaceRegister.Create(info)}
    }}

    {info.Accessor} void Serialize(ref Utf8YamlEmitter emitter, {info.Name} value, YamlSerializationContext context)
    {{
        if (value is null)
        {{
            emitter.WriteNull();
            return;
        }}
        if(context.IsMappingEnabled)
            emitter.BeginMapping();
        if(context.IsRedirected || context.IsFirst)
        {{
            emitter.Tag($""!{{typeof({info.Name})}},{{AssemblyName}}"");
            context.IsRedirected = false;
            context.IsFirst = false;
        }}
{serializerEmitter.Create(info)}
        if(context.IsMappingEnabled)
            emitter.EndMapping();
    }}

    {info.Accessor} {info.Name}? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
    {{
        
        return default!;
    }}
}}
";
            return Template;
        }



    }
}