using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace StrideSourceGenerator.Core.Roslyn;
internal class ClassInfo<T>
    where T : TypeDeclarationSyntax
{
    public GeneratorExecutionContext ExecutionContext { get; set; }
    public T TypeSyntax { get; set; }
    public ClassDeclarationSyntax SerializerSyntax { get; set; }
    public INamedTypeSymbol Symbol { get; set; }
    public AttributeData YamlObjectAttribute { get; set; }
    public string TypeName { get; set; }
    public string SerializerName { get; set; }
    public string NamespaceName { get; set; }
    public BFNNexSyntaxReceiver SyntaxReceiver { get; set; }
    public IEnumerable<IPropertySymbol> Properties
    {
        get
        {
            return PropertyFinder2.GetProperties(Symbol)
                           .Where(x => x is IPropertySymbol
                           {
                               IsStatic: false,
                               IsImplicitlyDeclared: false
                           })
                           .Where(y => y.SetMethod?.DeclaredAccessibility == Accessibility.Public || y.SetMethod?.DeclaredAccessibility == Accessibility.Internal)
                           .Where(z => z.GetMethod?.DeclaredAccessibility == Accessibility.Public || z.SetMethod?.DeclaredAccessibility == Accessibility.Internal);
        }
    }
    public IEnumerable<IFieldSymbol> Fields()
    {
        return PropertyFinder2.GetFields(Symbol)
               .Where(x => x is IFieldSymbol and { IsStatic: false, IsImplicitlyDeclared: false } && (x.DeclaredAccessibility == Accessibility.Public || x.DeclaredAccessibility == Accessibility.Internal));
    }

}
