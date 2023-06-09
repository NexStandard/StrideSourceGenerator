using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using StrideSourceGenerator.HelperClasses.Namespace;
using StrideSourceGenerator.HelperClasses.Methods;

namespace StrideSourceGenerator
{
    [Generator]
    public class BFNNexSourceGenerator : ISourceGenerator
    {
        PropertyAttributeFinder propertyFinder = new();
        public void Initialize(GeneratorInitializationContext context)
        {
         // Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new BFNNexSyntaxReceiver());
        }

        private NamespaceCreator NamespaceCreator { get; set; } = new();
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var syntaxReceiver = (BFNNexSyntaxReceiver)context.SyntaxReceiver;
                foreach (var classDeclaration in syntaxReceiver.ClassDeclarations)
                {
                    var className = GetClassName(classDeclaration);
                    // Retrieve the properties of the class, needs to be filtered to DataContract
                    var properties = propertyFinder.FilterProperties(classDeclaration.Members.OfType<PropertyDeclarationSyntax>());

                    var serializerClassName = $"GeneratedSerializer{className}";
                    var partialClass = SyntaxFactory.ClassDeclaration(serializerClassName)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

                    NamespaceDeclarationSyntax normalNamespace = NamespaceCreator.CreateNamespace(classDeclaration, context, className);
                    if (normalNamespace == null)
                    {
                        continue;
                    }

                    string namespaceName = normalNamespace.Name.ToString();

                    var inheritedProperties = propertyFinder.FilterInheritedProperties(classDeclaration, context);
                    AddMethodsToTheClass(className, ref partialClass, properties, className, inheritedProperties);

                    partialClass = AddInterfaces(partialClass, className);
                    if (normalNamespace == null)
                        continue;
                    var compilationUnit = SyntaxFactory.CompilationUnit()
                                                          .AddMembers(normalNamespace.AddMembers(partialClass));

                    var sourceText = compilationUnit.NormalizeWhitespace().ToFullString();

                    context.AddSource($"{serializerClassName}_{namespaceName}.g.cs", sourceText);

                }
            }
            catch (Exception ex)
            {
                // from manio143 MIT License
                context.ReportDiagnostic(Diagnostic.Create(
                    CompilerServicesUnhandledException,
                    Location.None,
                    ex.GetType().Name,
                    ex.ToString()));
                // end from manio143 MIT License
            }

        }
        private static ConvertToYamlMethodFactory writerFactory = new();
        private static DeserializeMethodFactory DeserializeMethodFactory = new();
        private static IdentifierTagFactory SerializedTypePropertyFactory = new ();
        private static IdentifierTypeFactory IdentifierTypeFactory = new ();
        private static void AddMethodsToTheClass(string className, ref ClassDeclarationSyntax partialClass, IEnumerable<PropertyDeclarationSyntax> properties, string serializerClassName, IEnumerable<IPropertySymbol> inheritedProperties)
        {
            var writeToDictionaryString = writerFactory.WriteToDictionaryTemplate(properties,serializerClassName,inheritedProperties);
            var deserializerMethodString = DeserializeMethodFactory.DeserializeMethodTemplate(properties, serializerClassName,inheritedProperties);
            var deserializerManyMethodString = DeserializeMethodFactory.DeserializeManyMethodTemplate(properties, serializerClassName,inheritedProperties);
            var deserializerFromYamlMappingNodeString = DeserializeMethodFactory.DeserializeFromYamlMappingNodeTemplate(properties, serializerClassName,inheritedProperties);
            var writeToDictionaryMethod = SyntaxFactory.ParseMemberDeclaration(writeToDictionaryString);
            var deserializeFromYamlMappingNodeMethod = SyntaxFactory.ParseMemberDeclaration(deserializerFromYamlMappingNodeString);
            var deserializeMethod = SyntaxFactory.ParseMemberDeclaration(deserializerMethodString);
            var identifierTagString = SerializedTypePropertyFactory.IdentifierTagTemplate(properties, serializerClassName, inheritedProperties);
            var identifierTypeString = IdentifierTypeFactory.IdentifierTagTemplate(properties,serializerClassName, inheritedProperties);
            partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(identifierTypeString));
            partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(identifierTagString));
            partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(deserializerManyMethodString));
            partialClass = partialClass.AddMembers(writeToDictionaryMethod);
            partialClass = partialClass.AddMembers(deserializeFromYamlMappingNodeMethod);
            partialClass = partialClass.AddMembers(deserializeMethod);
        }

        private static string GetClassName(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Identifier.ValueText;
        }
        private static ClassDeclarationSyntax AddInterfaces(ClassDeclarationSyntax partialClass, string className)
        {
            return partialClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlSerializer<{className}>")));
        }
        // from manio143 MIT License
        public const string CompilerServicesDiagnosticIdFormat = "STR0{0:000}";
        // from manio143 MIT License
        public const string CompilerServicesDiagnosticCategory = "Stride.CompilerServices";
        // from manio143 MIT License
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
        public List<ClassDeclarationSyntax> ClassDeclarations { get; private set; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            var result = finder.FindAttribute(syntaxNode);
            if (result != null)
            {
                ClassDeclarations.Add(result);
            }
        }
    }
}
