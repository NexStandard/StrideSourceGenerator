using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.API;
internal interface IAppendableTemplate
{
    void AppendTemplate(ClassInfo classInfo, StringBuilder builder);
}
