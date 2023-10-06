using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace StrideSourceGenerator.NexAPI;
internal class ClassInfo : IEquatable<ClassInfo>
{
    private const string GeneratorPrefix = "StrideSourceGenerated_";

    public static ClassInfo CreateFrom(ITypeSymbol type, ImmutableList<SymbolInfo> members)
    {
        string namespaceName = type.ContainingNamespace.Name;
        return new()
        {
            Name = type.Name,
            NameSpace = namespaceName,
            AllInterfaces = type.AllInterfaces.Select(t => t.Name).ToList(),
            AllAbstracts = FindAbstractClasses(type),
            Accessor = type.DeclaredAccessibility.ToString().ToLower(),
            GeneratorName = GeneratorPrefix + namespaceName + type.Name,
            MemberSymbols = members
        };
    }
    private ClassInfo() { }

    public string Name { get; set; }
    public string NameSpace { get; set; }
    public string GeneratorName { get; set; }
    public string Accessor { get; set; }
    internal IReadOnlyList<string> AllInterfaces { get; set; }
    internal IReadOnlyList<string> AllAbstracts { get; set; }
    public ImmutableList<SymbolInfo> MemberSymbols { get; internal set; }

    public bool Equals(ClassInfo other)
    {
        return Name == other.Name && NameSpace == other.NameSpace && GeneratorName == other.GeneratorName;
    }
    private static IReadOnlyList<string> FindAbstractClasses(ITypeSymbol typeSymbol)
    {
        List<string> result = new List<string>();
        INamedTypeSymbol baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.IsAbstract)
            {
                result.Add(baseType.Name);
            }
            baseType = baseType.BaseType;
        }
        return result;
    }
}