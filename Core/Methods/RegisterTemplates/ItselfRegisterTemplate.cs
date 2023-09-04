using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods.RegisterTemplates;
internal class ItselfRegisterTemplate : IAppendableTemplate
{
    public void AppendTemplate(ClassInfo classInfo, StringBuilder builder)
    {
        if (classInfo.Generics != null && classInfo.Generics.Parameters.Count > 0)
        {
            string str = new string(',', classInfo.Generics.Parameters.Count - 1);
            string generic = $"{classInfo.SerializerSyntax.Identifier.Text}<{str}>";
            string genericOfType = $"{classInfo.TypeName}<{str}>";
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterGenericFormatter(typeof({genericOfType}),typeof({generic}));");
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterFormatter(typeof({classInfo.TypeName + "<" + str + ">"}));");
        }
        else
        {
            builder.AppendLine($"NexYamlSerializerRegistry.Default.RegisterFormatter(this);");
        }
    }
}
