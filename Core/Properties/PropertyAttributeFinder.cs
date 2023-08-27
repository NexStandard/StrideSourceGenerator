using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace StrideSourceGenerator.Core.Properties;
internal class PropertyAttributeFinder
{
    List<string> allowedAttributes = new List<string>()
    {
       "DataMemberIgnore",
       "IgnoreDataMember"
    };

    /// <summary>
    /// Walks through a base class of a class and retrieves all allowed Properties.
    /// Then it tries to get it's own base class and get from it all allowed Properties recursively.
    /// All the Properties get summed up.
    /// </summary>
    /// <param name="currentBaseType">The base class which the ClassDeclarationSyntax has</param>
    /// <returns>All allowed Properties in any base class in the inheritance tree</returns>
    public static IEnumerable<IPropertySymbol> FilterBasePropertiesRecursive(ref INamedTypeSymbol currentBaseType)
    {
        var result = new List<IPropertySymbol>();
        while (currentBaseType != null)
        {
            result.AddRange(currentBaseType.GetMembers().OfType<IPropertySymbol>().Where(GetPropertiesWithAllowedAccessors()));
            currentBaseType = currentBaseType.BaseType;
        }
        return result;
    }
    /// <summary>
    /// Retrieves a delegate that filters properties based on their accessibility and accessor types.
    /// </summary>
    /// <returns>
    /// A <see cref="Func{IPropertySymbol, Boolean}"/> delegate that returns <c>true</c> for properties that have allowed accessors,
    /// and <c>false</c> otherwise.
    /// </returns>
    private static Func<IPropertySymbol, bool> GetPropertiesWithAllowedAccessors()
    {
        return propertyInfo =>
        {
            if (propertyInfo == null)
                return false;
            return (propertyInfo.SetMethod?.DeclaredAccessibility == Accessibility.Public ||
                    propertyInfo.SetMethod?.DeclaredAccessibility == Accessibility.Internal ||
                    propertyInfo.GetMethod?.ReturnsVoid == true
                )
                &&
                    (propertyInfo.GetMethod?.DeclaredAccessibility == Accessibility.Public ||
                    propertyInfo.GetMethod?.DeclaredAccessibility == Accessibility.Internal);

        };
    }
}