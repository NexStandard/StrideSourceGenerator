using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using StrideSourceGenerator.Core.GeneratorCreators;
using StrideSourceGenerator.Core.Roslyn;
using System.Diagnostics;
using System.Linq;
using StrideSourceGenerator.Core;

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
            ClassInfo info = new ClassInfo()
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

public class NexSyntaxReceiver : ISyntaxReceiver
{
    TypeAttributeFinder typeFinder = new();
    public List<TypeDeclarationSyntax> ClassDeclarations { get; private set; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        //ClassDeclarations.Clear();
        //StructDeclarations.Clear();
        TypeDeclarationSyntax result = typeFinder.FindAttribute(syntaxNode);

        if (result != null)
        {
            ClassDeclarations.Add(result);
        }
    }
}
