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
         //   AllInterfaces = ImmutableArray.Create(type.ContainingType.AllInterfaces.Select(t => t.Name).ToArray()),
         //   AllAbstracts = FindAbstractClasses(type),
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
    internal ImmutableArray<string> AllInterfaces { get; set; }
    internal ImmutableArray<string> AllAbstracts { get; set; }
    public List<SymbolInfo> MemberSymbols { get; internal set; }

    public bool Equals(ClassInfo other)
    {
        return Name == other.Name && NameSpace == other.NameSpace && GeneratorName == other.GeneratorName;
    }
    private static ImmutableArray<string> FindAbstractClasses(ITypeSymbol typeSymbol)
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
        return ImmutableArray.Create(result.ToArray());
    }
}