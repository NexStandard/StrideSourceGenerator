using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using StrideSourceGenerator.Core.GeneratorCreators;

namespace StrideSourceGenerator
{
    [Generator]
    public class BFNNexSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
         // Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new BFNNexSyntaxReceiver());
        }
        private GeneratorYamlClass classGenerator { get; set; } = new();
        private GeneratorYamlStruct structGenerator { get; set; } = new();
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var syntaxReceiver = (BFNNexSyntaxReceiver)context.SyntaxReceiver;
                
                foreach (var classDeclaration in syntaxReceiver.ClassDeclarations)
                {
                    classGenerator.StartCreation(context, classDeclaration,syntaxReceiver);
                }
                foreach(var structDeclaration in syntaxReceiver.StructDeclarations)
                {
                    structGenerator.StartCreation(context, structDeclaration,syntaxReceiver);
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
            var result = finder.FindAttribute(syntaxNode);
            if (result != null)
            {
                ClassDeclarations.Add(result);
            }
            var structResult = structFinder.FindAttribute(syntaxNode);
            if (structResult != null)
            {
                StructDeclarations.Add(structResult);
            }
        }
    }
}
