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

            var syntaxReceiver = (BFNNexSyntaxReceiver)context.SyntaxReceiver;
            foreach (var classDeclaration in syntaxReceiver.ClassDeclarations)
            {
                var className = GetClassName(classDeclaration);
                // Retrieve the properties of the class, needs to be filtered to DataContract
                var properties = propertyFinder.FilterProperties(classDeclaration.Members.OfType<PropertyDeclarationSyntax>());

                var serializerClassName = $"GeneratedSerializer{className}";
                var partialClass = SyntaxFactory.ClassDeclaration(serializerClassName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

                NamespaceDeclarationSyntax normalNamespace = NamespaceCreator.CreateNamespace(classDeclaration,context,className);
                if(normalNamespace == null)
                {
                    continue;
                }

                string namespaceName = normalNamespace.Name.ToString();
                
                var inheritedProperties = propertyFinder.FilterInheritedProperties(classDeclaration,context);
                AddMethodsToTheClass(className, ref partialClass,properties,className,inheritedProperties);

                partialClass = AddInterfaces(partialClass, className);
                if (normalNamespace == null)
                    continue;
                var compilationUnit = SyntaxFactory.CompilationUnit()
                                                      .AddMembers(normalNamespace.AddMembers(partialClass));

                var sourceText = compilationUnit.NormalizeWhitespace().ToFullString();

                context.AddSource($"{serializerClassName}_{namespaceName}.g.cs", sourceText);

            }
        }
        private static ConvertToYamlMethodFactory writerFactory = new();
        private static SerializedTypePropertyFactory SerializedTypePropertyFactory = new ();
        private static void AddMethodsToTheClass(string className, ref ClassDeclarationSyntax partialClass, IEnumerable<PropertyDeclarationSyntax> properties, string serializerClassName, IEnumerable<IPropertySymbol> inheritedProperties)
        {
            var writeToDictionaryString = writerFactory.WriteToDictionaryTemplate(properties,serializerClassName,inheritedProperties);
            var writeToDictionaryMethod = SyntaxFactory.ParseMemberDeclaration(writeToDictionaryString);
            var serializedTypePropertyString = SerializedTypePropertyFactory.SerializedTypeProperty(properties, serializerClassName, inheritedProperties);
            partialClass = partialClass.AddMembers(SyntaxFactory.ParseMemberDeclaration(serializedTypePropertyString));
            partialClass = partialClass.AddMembers(writeToDictionaryMethod);
        }

        private static string GetClassName(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Identifier.ValueText;
        }
        private static ClassDeclarationSyntax AddInterfaces(ClassDeclarationSyntax partialClass, string className)
        {
            return partialClass;
     //       return partialClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlSerializer<{className}>")));
        }
    }

    class BFNNexSyntaxReceiver : ISyntaxReceiver
    {
        ClassAttributeFinder finder = new();
        public List<ClassDeclarationSyntax> ClassDeclarations { get; private set; } = new List<ClassDeclarationSyntax>();
        public ClassDeclarationSyntax WriterRegistryClass { get; private set; }
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
