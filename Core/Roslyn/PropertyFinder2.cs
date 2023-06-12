using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Roslyn;
internal class PropertyFinder2
{
    public static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol symbol, bool withoutOverride = true)
    {
        return GetAllMembers<IPropertySymbol>(symbol, withoutOverride);
    }
    public static IEnumerable<IFieldSymbol> GetFields(INamedTypeSymbol symbol, bool withoutOverride = true)
    {
        return GetAllMembers<IFieldSymbol>(symbol, withoutOverride);
    }
    private static IEnumerable<T> GetAllMembers<T>(INamedTypeSymbol symbol, bool withoutOverride = true)
        where T : ISymbol
    {
        // Iterate Parent -> Derived
        if (symbol.BaseType != null)
        {
            foreach (var item in GetAllMembers<T>(symbol.BaseType))
            {

                // override item already iterated in parent type
                if (!withoutOverride || !item.IsOverride)
                {
                    if(item is T property)
                        yield return property;
                }
            }
        }

        foreach (var item in symbol.GetMembers())
        {
            if (!withoutOverride || !item.IsOverride)
            {
                if(item is T property)
                    yield return property;
            }
        }
    }
}
