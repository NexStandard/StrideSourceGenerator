using StrideSourceGenerator.NexAPI.MemberSymbolAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace StrideSourceGenerator.NexAPI.Analysation.Analyzers;
internal class FieldAnalyzer : IMemberSymbolAnalyzer<IFieldSymbol>
{
    protected readonly IContentModeInfo memberGenerator;
    internal FieldAnalyzer(IContentModeInfo memberGenerator)
    {
        this.memberGenerator = memberGenerator;
    }

    public SymbolInfo Analyze(MemberContext<IFieldSymbol> context)
    {
        return new SymbolInfo()
        {
            Name = context.Symbol.Name,
            MemberGenerator = memberGenerator,
            Type = context.Symbol.Type.ContainingNamespace.Name + "." + context.Symbol.Type.Name
        };
    }

    public bool AppliesTo(MemberContext<IFieldSymbol> symbol) => true;
}
