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
                
                return s.GetMembers().OfType<IPropertySymbol>();
            }

        }
        return Enumerable.Empty<IPropertySymbol>();

    }
    private List<IPropertySymbol> FilterBasePropertiesRecursive(INamedTypeSymbol s)
    {
        if (s.BaseType != null)
        {
            List<IPropertySymbol> baseProperties = new List<IPropertySymbol>();
            baseProperties.AddRange(s.GetMembers().OfType<IPropertySymbol>());
        }
        return new List<IPropertySymbol>();
    }
}
