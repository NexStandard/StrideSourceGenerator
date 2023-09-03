using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using StrideSourceGenerator.Core.GeneratorCreators;
using StrideSourceGenerator.Core.Roslyn;
using System.Diagnostics;
using System.Linq;

namespace StrideSourceGenerator;

[Generator]
public class NexGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Debugger.Launch();
        context.RegisterForSyntaxNotifications(() => new NexSyntaxReceiver());
    }
    private GeneratorYamlClass classGenerator { get; set; } = new();
    public void Execute(GeneratorExecutionContext context)
    {
        NexSyntaxReceiver syntaxReceiver = (NexSyntaxReceiver)context.SyntaxReceiver;

        foreach (TypeDeclarationSyntax classDeclaration in syntaxReceiver.ClassDeclarations)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            ClassInfo info = new()
            {
                ExecutionContext = context,
                TypeSyntax = classDeclaration,
                SyntaxReceiver = syntaxReceiver,
                Symbol = semanticModel.GetDeclaredSymbol(classDeclaration),

            };
            classGenerator.StartCreation(info);
        }
    }

    public const string CompilerServicesDiagnosticIdFormat = "STR0{0:000}";

    public const string CompilerServicesDiagnosticCategory = "Stride.CompilerServices";
}
