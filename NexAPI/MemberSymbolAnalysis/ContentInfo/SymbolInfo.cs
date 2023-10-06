using StrideSourceGenerator.NexAPI;

internal class SymbolInfo
{
    internal virtual bool IsEmpty { get; } = false;
    internal string Name { get; set; }
    internal string Type { get; set; }
    internal string Namespace { get; set; }
    internal IContentModeInfo MemberGenerator { get; set; }
    internal bool IsGeneric { get; set; }
}