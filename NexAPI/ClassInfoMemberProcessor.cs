﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using StrideSourceGenerator.Core;
using StrideSourceGenerator.NexAPI.Core;
using StrideSourceGenerator.NexAPI.MemberSymbolAnalysis;
using System.Collections.Immutable;

namespace StrideSourceGenerator.NexAPI;
internal class ClassInfoMemberProcessor(IMemberSelector selector, Compilation compilation)
{
    public List<IMemberSymbolAnalyzer<IPropertySymbol>> PropertyAnalyzers { get; set; } = new();
    public List<IMemberSymbolAnalyzer<IFieldSymbol>> FieldAnalyzers { get; set; } = new();
    private INamedTypeSymbol dataMemberAttribute;
    private INamedTypeSymbol dataMembermode;
    private INamedTypeSymbol DataMemberAttribute =>  WellKnownReferences.DataMemberAttribute(compilation);
    private INamedTypeSymbol DataMemberMode => WellKnownReferences.DataMemberMode(compilation);
    public List<SymbolInfo> Process(ITypeSymbol type)
    {
        IReadOnlyList<ISymbol> symbols = selector.GetAllMembers(type);
        List<SymbolInfo> result = new List<SymbolInfo>();
        foreach (ISymbol symbol in symbols)
        {
            DataMemberContext context = DataMemberContext.Create(symbol, DataMemberAttribute, DataMemberMode);
            if (symbol == null)
                continue;
            if (symbol is IPropertySymbol property)
                ProcessAnalyzers(PropertyAnalyzers, property, result, context);
            else if (symbol is IFieldSymbol field)
            {
                ProcessAnalyzers(FieldAnalyzers, field, result, context);
            }
        }
        return result.ToList();
    }
    void ProcessAnalyzers<T>(List<IMemberSymbolAnalyzer<T>> analyzers, T symbol, List<SymbolInfo> result, DataMemberContext context)
        where T : ISymbol
    {
        foreach (IMemberSymbolAnalyzer<T> analyzer in analyzers)
        {
            MemberContext<T> memberContext = new MemberContext<T>(symbol, context);

            SymbolInfo temp = analyzer.Analyze(memberContext);
            
                result.Add(temp);
        }
    }
}
