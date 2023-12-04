using StrideSourceGenerator.NexAPI.MemberSymbolAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using StrideSourceGenerator.NexAPI.Core;

namespace StrideSourceGenerator.NexAPI.Analysation.Analyzers
{
    internal class FieldAnalyzer : IMemberSymbolAnalyzer<IFieldSymbol>
    {
        protected readonly IContentModeInfo memberGenerator;
        internal FieldAnalyzer(IContentModeInfo memberGenerator)
        {
            this.memberGenerator = memberGenerator;
        }

        public SymbolInfo Analyze(MemberContext<IFieldSymbol> context)
        {
            var names = context.Symbol.Type.ContainingNamespace;
            string namespa = context.Symbol.Type.Name;
            if (names != null)
            {
                namespa = context.Symbol.Type.GetFullNamespace('.') + "." + context.Symbol.Type.Name;
            }
            return new SymbolInfo()
            {
                Name = context.Symbol.Name,
                TypeKind = SymbolKind.Field,
                MemberGenerator = memberGenerator,
                Type = namespa,
                IsAbstract = context.Symbol.Type.IsAbstract,
                IsInterface = context.Symbol.Type.TypeKind == TypeKind.Interface,
                Context = context.DataMemberContext,

            };
        }

        public bool AppliesTo(MemberContext<IFieldSymbol> symbol) => true;
    }
}