using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using StrideSourceGenerator.Core.GeneratorCreators;
using StrideSourceGenerator.Core.Roslyn;
using System.Diagnostics;
using StrideSourceGenerator.AttributeFinder;

namespace StrideSourceGenerator
{
    [Generator]
    public class NexGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new BFNNexSyntaxReceiver());
        }
        private GeneratorYamlClass classGenerator { get; set; } = new();
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                BFNNexSyntaxReceiver syntaxReceiver = (BFNNexSyntaxReceiver)context.SyntaxReceiver;
                
                foreach (ClassDeclarationSyntax classDeclaration in syntaxReceiver.ClassDeclarations)
                {
                    SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                    ClassInfo<ClassDeclarationSyntax> info = new ClassInfo<ClassDeclarationSyntax>()
                    {
                        ExecutionContext = context,
                        TypeSyntax = classDeclaration,
                        SyntaxReceiver = syntaxReceiver,
                        Symbol = semanticModel.GetDeclaredSymbol(classDeclaration),
                        
                    };
                    classGenerator.StartCreation(info);
                }
                foreach(StructDeclarationSyntax structDeclaration in syntaxReceiver.StructDeclarations)
                {
                    // classGenerator.StartCreation(context, structDeclaration,syntaxReceiver);
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

    class BFNNexSyntaxReceiver : ISyntaxReceiver
    {
        ClassAttributeFinder finder = new();
        StructAttributeFinder structFinder = new();
        public List<ClassDeclarationSyntax> ClassDeclarations { get; private set; } = new ();
        public List<StructDeclarationSyntax> StructDeclarations { get; private set; } = new ();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            ClassDeclarations.Clear();
            StructDeclarations.Clear();
            ClassDeclarationSyntax result = finder.FindAttribute(syntaxNode);
            if (result != null)
            {
                ClassDeclarations.Add(result);
            }
            StructDeclarationSyntax structResult = structFinder.FindAttribute(syntaxNode);
            if (structResult != null)
            {
                StructDeclarations.Add(structResult);
            }
        }
    }
}
