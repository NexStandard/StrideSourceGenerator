using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;

namespace StrideSourceGenerator;
internal class PropertyAttributeFinder
{
    List<String> allowedAttributes = new List<string>()
    {
       // typeof(Stride.Core.DataMember),
       "DataMember",
       "DataMemberAttribute",
        "System.Runtime.Serialization.DataMemberAttribute",
        "System.Runtime.Serialization.DataMember"
    };
    public IEnumerable<PropertyDeclarationSyntax> FilterProperties(IEnumerable<PropertyDeclarationSyntax> properties)
    {
        return properties.Where(x =>
        {
            var attributes = x.AttributeLists.SelectMany(b => b.Attributes);
            foreach(var attribute in attributes)
            {
                if(allowedAttributes.Contains(attribute.Name.ToString()))
                    return true;
            }
            return false;
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
        var baseList = declarationSyntax.BaseList;
        if (baseList == null)
            return properties1;
        if(baseList.Types.Count != 0)
        {
            var s = (INamedTypeSymbol)model.GetSymbolInfo(baseList.Types[0].Type).Symbol;
            
            if (s != null)
            {

                return FilterBasePropertiesRecursive(ref s);
            }

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
    private IEnumerable<IPropertySymbol> FilterBasePropertiesRecursive(ref INamedTypeSymbol currentBaseType)
    {
        var nextBaseType = currentBaseType.BaseType;
        var result = new List<IPropertySymbol>() ;
        if(currentBaseType.BaseType != null)
        {
            result.Concat(FilterBasePropertiesRecursive(ref nextBaseType));
        }
        var publicGetterProperties  = result.Concat(currentBaseType.GetMembers().OfType<IPropertySymbol>().Where(s => s.GetMethod?.DeclaredAccessibility == Accessibility.Public));
        return publicGetterProperties;
    }
}