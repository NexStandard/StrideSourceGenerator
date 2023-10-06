using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace StrideSourceGenerator.NexAPI;
internal class ClassInfo : IEquatable<ClassInfo>
{
    private const string GeneratorPrefix = "StrideSourceGenerated_";

    public static ClassInfo CreateFrom(ITypeSymbol type, ClassInfoMemberProcessor processor)
    {
        string namespaceName = type.ContainingNamespace.Name;
        return new()
        {
            Name = type.Name,
            NameSpace = namespaceName,
            Accessor = type.DeclaredAccessibility.ToString().ToLower(),
            GeneratorName = GeneratorPrefix + namespaceName + type.Name,
            MemberSymbols = processor.Process(type)
        };
    }
    private ClassInfo() { }

    public string Name { get; set; }
    public string NameSpace { get; set; }
    public string GeneratorName { get; set; }
    public string Accessor { get; set; }
    public List<SymbolInfo> MemberSymbols { get; internal set; }

    public bool Equals(ClassInfo other)
    {
        return Name == other.Name && NameSpace == other.NameSpace && GeneratorName == other.GeneratorName;
    }
}