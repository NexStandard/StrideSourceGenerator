using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace StrideSourceGenerator
{
    [Generator]
    public class BFNNexSourceGenerator : ISourceGenerator
    {
        PropertyAttributeFinder propertyFinder = new();
        UsingDirectiveProvider UsingDirectiveProvider { get; set; } = new();
        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new BFNNexSyntaxReceiver());

        }
        public static NamespaceDeclarationSyntax GetNamespaceFrom(SyntaxNode s) =>
        s.Parent switch
        {
            NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax,
            null => null,
            _ => GetNamespaceFrom(s.Parent)
        };

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

                var normalNamespace = GetNamespaceFrom(classDeclaration);
                // without this line, everything breaks apart
                normalNamespace = normalNamespace.RemoveNode(classDeclaration, SyntaxRemoveOptions.KeepNoTrivia);

                normalNamespace = UsingDirectiveProvider.AddUsingDirectives(normalNamespace);
                
                var inheritedProperties = propertyFinder.FilterInheritedProperties(classDeclaration,context);
                var DefaultSettings = AddMethodsToTheClass(className, ref partialClass,properties,className,inheritedProperties);

                partialClass = partialClass.AddMembers(DefaultSettings);
                if (normalNamespace == null)
                    continue;
                var compilationUnit = SyntaxFactory.CompilationUnit()
                                                      .AddMembers(normalNamespace.AddMembers(partialClass));
                var sourceText = compilationUnit.NormalizeWhitespace().ToFullString();

                context.AddSource($"{serializerClassName}.g.cs", sourceText);

            }
        }
        private static WriterFactory writerFactory = new();
        private static MemberDeclarationSyntax AddMethodsToTheClass(string className, ref ClassDeclarationSyntax partialClass, IEnumerable<PropertyDeclarationSyntax> properties, string serializerClassName, IEnumerable<IPropertySymbol> inheritedProperties)
        {
            var writeToDictionaryString = writerFactory.WriteToDictionaryTemplate(properties,serializerClassName,inheritedProperties);
            var writeToDictionaryMethod = SyntaxFactory.ParseMemberDeclaration(writeToDictionaryString);
            return writeToDictionaryMethod;
        }

        private static string GetClassName(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Identifier.ValueText;
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
