using Microsoft.CodeAnalysis;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.API;
internal interface IPropertyAppendableTemplate
{
    void AppendTemplate(IPropertySymbol propertySymbol, StringBuilder builder);
}
