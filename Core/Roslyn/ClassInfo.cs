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
    public TypeParameterListSyntax Generics => TypeSyntax.TypeParameterList;

    public INamedTypeSymbol Symbol { get; set; }
    public string TypeName { get; set; }
    public string SerializerName { get; set; }
    public string NamespaceName { get; set; }
    public BFNNexSyntaxReceiver SyntaxReceiver { get; set; }
}
