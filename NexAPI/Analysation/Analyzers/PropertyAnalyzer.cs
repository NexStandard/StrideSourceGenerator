using Microsoft.CodeAnalysis;
using StrideSourceGenerator.NexAPI.MemberSymbolAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.NexAPI.Analysation.Analyzers;
internal class PropertyAnalyzer : IMemberSymbolAnalyzer<IPropertySymbol>
{
    protected readonly IContentModeInfo memberGenerator;
    internal PropertyAnalyzer(IContentModeInfo memberGenerator)
    {
        this.memberGenerator = memberGenerator;
    }

    public SymbolInfo Analyze(MemberContext<IPropertySymbol> symbol)
    {
        return new SymbolInfo()
        {
            Name = symbol.Symbol.Name,
            MemberGenerator = memberGenerator,
            Type = symbol.Symbol.Type.ContainingNamespace.Name + "." + symbol.Symbol.Type.Name
        };
    }

    public bool AppliesTo(MemberContext<IPropertySymbol> symbol) => true;
}

