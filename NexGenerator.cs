using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using StrideSourceGenerator.Core.GeneratorCreators;
using StrideSourceGenerator.Core.Roslyn;
using System.Diagnostics;
using StrideSourceGenerator.AttributeFinder;
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
        try
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
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                CompilerServicesUnhandledException,
                Location.None,
                ex.GetType().Name,
                ex.ToString()));
        }

    }




    public const string CompilerServicesDiagnosticIdFormat = "STR0{0:000}";

    public const string CompilerServicesDiagnosticCategory = "Stride.CompilerServices";

    public static DiagnosticDescriptor CompilerServicesUnhandledException = new DiagnosticDescriptor(
        string.Format(CompilerServicesDiagnosticIdFormat, 1),
        "An unhandled exception occurred",
        "An {0} occurred while running Stride.Core.CompilerServices analyzer. {1}.",
        CompilerServicesDiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
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
