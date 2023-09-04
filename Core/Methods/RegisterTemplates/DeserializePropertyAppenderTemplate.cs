using Microsoft.CodeAnalysis;
using StrideSourceGenerator.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods.RegisterTemplates;
internal class DeserializePropertyAppenderTemplate : IPropertyAppendableTemplate
{
    public bool IsFirstTime { get; set; }

    public void AppendTemplate(IPropertySymbol propertySymbol, StringBuilder builder)
    {
        builder.Append($$"""
                        {{GetStart()}} (key.SequenceEqual({{"UTF8" + propertySymbol.Name}}))
                        {
                            parser.Read();
                            temp{{propertySymbol.Name}} = context.DeserializeWithAlias<{{propertySymbol.Type}}>(ref parser);
                        }
                        """);

    }
    string GetStart() => IsFirstTime switch
    {
        true => "if",
        false => "else if"
    };
}
