using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Templates;
internal class Constants
{
    public const string SerializerRegistry = "NexYamlSerializerRegistry.Instance";
    public const string RegisterFormatter = ".RegisterFormatter({0});";
}
