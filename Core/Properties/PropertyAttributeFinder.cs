using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;

namespace StrideSourceGenerator.Core.Properties;
internal class PropertyAttributeFinder
{
    List<string> allowedAttributes = new List<string>()
    {
       "DataMemberIgnore",
       "IgnoreDataMember"
    };
    public IEnumerable<PropertyDeclarationSyntax> FilterProperties(GeneratorExecutionContext context, IEnumerable<PropertyDeclarationSyntax> properties)
    {
        return properties.Where(property =>
        {
            IEnumerable<AttributeSyntax> attributes = property.AttributeLists.SelectMany(b => b.Attributes);

            if (attributes.Any(attribute => allowedAttributes.Contains(attribute.Name.ToString())))
                return false;

            IPropertySymbol propertyInfo = context.Compilation.GetSemanticModel(property.SyntaxTree).GetDeclaredSymbol(property) as IPropertySymbol;

            return GetPropertiesWithAllowedAccessors().Invoke(propertyInfo);
        });
    }


    public IEnumerable<IPropertySymbol> FilterInheritedProperties(ClassDeclarationSyntax declarationSyntax, GeneratorExecutionContext context)
    {

        // Get the syntax tree from the declaration syntax
        SyntaxTree tree = declarationSyntax.SyntaxTree;

        // Get the semantic model from the compilation
        Compilation compilation = context.Compilation;
        SemanticModel model = compilation.GetSemanticModel(tree);
        IEnumerable<IPropertySymbol> properties1 = Enumerable.Empty<IPropertySymbol>();
        BaseListSyntax baseList = declarationSyntax.BaseList;
        if (baseList == null)
            return properties1;
        if (baseList.Types.Count != 0)
        {
            INamedTypeSymbol s = (INamedTypeSymbol)model.GetSymbolInfo(baseList.Types[0].Type).Symbol;

            if (s != null)

                return FilterBasePropertiesRecursive(ref s);

        }
        return Enumerable.Empty<IPropertySymbol>();

    }
    /// <summary>
    /// Walks through a base class of a class and retrieves all allowed Properties.
    /// Then it tries to get it's own base class and get from it all allowed Properties recursively.
    /// All the Properties get summed up.
    /// </summary>
    /// <param name="currentBaseType">The base class which the ClassDeclarationSyntax has</param>
    /// <returns>All allowed Properties in any base class in the inheritance tree</returns>
    public IEnumerable<IPropertySymbol> FilterBasePropertiesRecursive(ref INamedTypeSymbol currentBaseType)
    {
        INamedTypeSymbol nextBaseType = currentBaseType.BaseType;
        List<IPropertySymbol> result = new List<IPropertySymbol>();
        if (currentBaseType.BaseType != null)
            result.Concat(FilterBasePropertiesRecursive(ref nextBaseType));
        IEnumerable<IPropertySymbol> publicGetterProperties = result.Concat(currentBaseType.GetMembers().OfType<IPropertySymbol>().Where(GetPropertiesWithAllowedAccessors()));
        return publicGetterProperties;
    }

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