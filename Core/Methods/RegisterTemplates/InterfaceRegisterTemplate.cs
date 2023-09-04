using Microsoft.CodeAnalysis;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods.RegisterTemplates;
internal class InterfaceRegisterTemplate : IAppendableTemplate
{
    public void AppendTemplate(ClassInfo classInfo, StringBuilder builder)
    {
        System.Collections.Immutable.ImmutableArray<INamedTypeSymbol> interfaces = classInfo.Symbol.AllInterfaces;
        foreach (INamedTypeSymbol inter in interfaces)
        {
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterInterface(this,typeof({inter.Name}));");
        }
    }
}
