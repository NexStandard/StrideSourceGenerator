using Microsoft.CodeAnalysis;
using StrideSourceGenerator.NexAPI.MemberSymbolAnalysis;

namespace StrideSourceGenerator.NexAPI.PreProcessor.Analyzers;
internal class BaseAnalyzer<T> : IMemberSymbolAnalyzer<T>
    where T : ISymbol
{
    protected readonly IContentModeInfo memberGenerator;
    internal BaseAnalyzer(IContentModeInfo memberGenerator)
    {
        this.memberGenerator = memberGenerator;
    }

    public virtual SymbolInfo Analyze(MemberContext<T> context)
    {
        string type = "";
        string @namespace = "";
        if (context.Symbol is IPropertySymbol property)
        {
            type = property.Type.Name;
            @namespace = property.Type.ContainingNamespace.Name;
        }
        if (context.Symbol is IFieldSymbol field)
        {
            type = field.Type.Name;
            @namespace = field.Type.ContainingNamespace.Name;
        }
        return new SymbolInfo()
        {
            Name = context.Symbol.Name,
            MemberGenerator = memberGenerator,
            Type = @namespace + "." + type
        };
    }
    public virtual bool AppliesTo(MemberContext<T> symbol) => true;
}
