using Microsoft.CodeAnalysis;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods.RegisterTemplates;
internal class AbstractClassRegister : IAppendableTemplate
{
    public void AppendTemplate(ClassInfo classInfo, StringBuilder builder)
    {
        ITypeSymbol classSymbol = classInfo.Symbol;
        if (classSymbol.IsAbstract)
        {
            return;
        }
        INamedTypeSymbol currentBaseType = classSymbol.BaseType;
        while (currentBaseType != null)
        {
            if (currentBaseType.IsAbstract)
            {
                builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterAbstractClass(this,typeof({currentBaseType.Name}));");
            }
            currentBaseType = currentBaseType.BaseType;
        }
    }
}
