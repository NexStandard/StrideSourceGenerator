using Microsoft.CodeAnalysis;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods.RegisterTemplates;

internal class DeserializeArrayAppenderTemplate : IPropertyAppendableTemplate
{
    public bool IsFirstTime { get; set; }

    public void AppendTemplate(IPropertySymbol propertySymbol, StringBuilder builder)
    {
        IArrayTypeSymbol arrayType = (IArrayTypeSymbol)propertySymbol.Type;
        string start = GetStart();
        if (arrayType.ElementType.SpecialType == SpecialType.System_Byte)
        {
            builder.Append($$"""
                            {{start}} (key.SequenceEqual({{"UTF8" + propertySymbol.Name}}))
                            {
                                parser.Read();
                                temp{{propertySymbol.Name}} = context.DeserializeByteArray(ref parser);
                            }
                            """);
        }
        else
        {
            builder.Append($$"""
                            {{start}} (key.SequenceEqual({{"UTF8" + propertySymbol.Name}}))
                            {
                                parser.Read();
                                temp{{propertySymbol.Name}} = context.DeserializeArray<{{arrayType.ElementType}}>(ref parser);
                            }
                            """);
        }
        IsFirstTime = false;
    }

    string GetStart() => IsFirstTime switch
    {
        true => "if",
        false => "else if"
    };

}
