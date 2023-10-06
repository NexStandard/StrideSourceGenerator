using Microsoft.CodeAnalysis;
using StrideSourceGenerator.NexAPI.Core;
using System.Runtime.Serialization;

namespace StrideSourceGenerator.NexAPI.MemberSymbolAnalysis;

internal class DataMemberContext
{
    private DataMemberContext() { }
    internal static DataMemberContext Create(ISymbol symbol, INamedTypeSymbol dataMemberAttribute, INamedTypeSymbol DataMemberMode)
    {
        DataMemberContext context = new DataMemberContext();
        if (symbol.TryGetAttribute(dataMemberAttribute, out AttributeData attributeData))
        {
            context.Exists = true;
            context.Mode = GetDataMemberMode(attributeData, DataMemberMode);
            context.Order = 0;
        }
        else
        {
            context.Exists = false;
        }
        return context;
    }
    static int GetDataMemberMode(AttributeData attributeData, INamedTypeSymbol dataMemberAttribute)
    {
        TypedConstant modeParameter = attributeData.ConstructorArguments.FirstOrDefault(x => x.Type?.Equals(dataMemberAttribute, SymbolEqualityComparer.Default) ?? false);

        if (modeParameter.Value is null)
            return 0;
        return (int)modeParameter.Value;
    }
    public bool Exists { get; set; }
    public int Mode { get; set; }
    public int Order { get; set; }
}
