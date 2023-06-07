using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace StrideSourceGenerator
{
    [Generator]
    public class BFNNexSourceGenerator : ISourceGenerator
    {
        PropertyAttributeFinder propertyFinder = new();
        UsingDirectiveProvider UsingDirectiveProvider { get; set; } = new();
        public void Initialize(GeneratorInitializationContext context)
        {
         // Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new BFNNexSyntaxReceiver());

        }
        public static NamespaceDeclarationSyntax GetNamespaceFrom(SyntaxNode s)
        {
            while (s != null)
            {
                if (s is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
                {
                    var newNamespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
                        fileScopedNamespaceDeclarationSyntax.AttributeLists,
                        fileScopedNamespaceDeclarationSyntax.Modifiers,
                        fileScopedNamespaceDeclarationSyntax.NamespaceKeyword,
                        fileScopedNamespaceDeclarationSyntax.Name,
                        SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(fileScopedNamespaceDeclarationSyntax.SemicolonToken.TrailingTrivia),
                        fileScopedNamespaceDeclarationSyntax.Externs,
                        fileScopedNamespaceDeclarationSyntax.Usings,
                        fileScopedNamespaceDeclarationSyntax.Members,
                        SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
                        semicolonToken: default);
                    var classes = newNamespaceDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>();
                    foreach (var classDeclarationNode in classes)
                    {
                        newNamespaceDeclaration = newNamespaceDeclaration.RemoveNode(classDeclarationNode, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                    return newNamespaceDeclaration;
                }
                else if (s is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                {
                    var classes =namespaceDeclarationSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>();
                    foreach (var classDeclarationNode in classes )
                    {
                        namespaceDeclarationSyntax = namespaceDeclarationSyntax.RemoveNode(classDeclarationNode, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                    return namespaceDeclarationSyntax;
                }

                s = s.Parent;
            }

            return null;
        }
        /*
public static NamespaceDeclarationSyntax GetNamespaceFrom(SyntaxNode s) =>
s.Parent switch
{
    NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax,
    null => null,
    _ => GetNamespaceFrom(s.Parent)
};
*/

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
               //  normalNamespace = normalNamespace.RemoveNode(classDeclaration, SyntaxRemoveOptions.KeepNoTrivia);

                normalNamespace = UsingDirectiveProvider.AddUsingDirectives(normalNamespace);
                
                var inheritedProperties = propertyFinder.FilterInheritedProperties(classDeclaration,context);
                AddMethodsToTheClass(className, ref partialClass,properties,className,inheritedProperties);

                partialClass = AddInterfaces(partialClass, className);
                if (normalNamespace == null)
                    continue;
                var compilationUnit = SyntaxFactory.CompilationUnit()
                                                      .AddMembers(normalNamespace.AddMembers(partialClass));
                var sourceText = compilationUnit.NormalizeWhitespace().ToFullString();

                context.AddSource($"{serializerClassName}.g.cs", sourceText);

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
