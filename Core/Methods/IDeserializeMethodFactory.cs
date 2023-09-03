using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using StrideSourceGenerator.Core.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Methods;
public interface IDeserializeMethodFactory
{
    public MemberDeclarationSyntax GetMethod(ClassInfo classInfo);
    
}
public static class DeserializeMethodExtension
{
    public static Dictionary<int, List<IPropertySymbol>> MapPropertiesToLength(this IDeserializeMethodFactory factory,IEnumerable<IPropertySymbol> properties)
    {
        Dictionary<int, List<IPropertySymbol>> map = new();
        foreach (IPropertySymbol property in properties)
        {
            int propertyLength = property.Name.Length;
            if (!map.ContainsKey(propertyLength))
            {
                map.Add(propertyLength, new() { property });
            }
            else
            {
                map[propertyLength].Add(property);
            }
        }
        return map;
    }
}