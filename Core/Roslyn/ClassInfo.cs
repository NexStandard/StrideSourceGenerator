using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using StrideSourceGenerator.Core.Properties;

namespace StrideSourceGenerator.Core.Roslyn;
public class ClassInfo<T>
    where T : TypeDeclarationSyntax
{
    public GeneratorExecutionContext ExecutionContext { get; set; }
    public T TypeSyntax { get; set; }
    public ClassDeclarationSyntax SerializerSyntax { get; set; }
    public TypeParameterListSyntax Generics => TypeSyntax.TypeParameterList;

    public INamedTypeSymbol Symbol { get; set; }
    public string TypeName { get; set; }
    public string SerializerName { get; set; }
    public string NamespaceName { get; set; }
    public BFNNexSyntaxReceiver SyntaxReceiver { get; set; }
    public SemanticModel SemanticModel { get => _compilationCache ??= ExecutionContext.Compilation.GetSemanticModel(TypeSyntax.SyntaxTree); }
    private SemanticModel _compilationCache;
    private List<IPropertySymbol> _propertyCache;
    public IEnumerable<IPropertySymbol> AvailableProperties { get
        {
            if (_propertyCache == null)
            {
                var classSymbol = SemanticModel.GetDeclaredSymbol(TypeSyntax);
                var properties = PropertyAttributeFinder.FilterBasePropertiesRecursive(ref classSymbol);
                return _propertyCache = properties.ToList();
            } else {
                return _propertyCache;
            }
        }
     }

}
