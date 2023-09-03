using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Methods;
using StrideSourceGenerator.Core.Properties;
using StrideSourceGenerator.Core.Roslyn;
using StrideSourceGenerator.Core.Templates;
using System.Linq;
using static StrideSourceGenerator.API.NamespaceProvider;

namespace StrideSourceGenerator.Core.GeneratorCreators;

internal abstract class GeneratorBase
{
    protected ITemplateProvider TagTemplate = new TagTemplateProvider();
    protected ITemplateProvider TypeTemplate = new TypeTemplateProvider();
    protected PropertyAttributeFinder PropertyFinder { get; } = new();
    private NamespaceProvider NamespaceProvider { get; }
    protected SerializeMethodFactory writerFactory;
    protected DeserializeMethodFactory DeserializeMethodFactory = new();
    protected RegisterMethodFactory RegisterMethodFactory { get; } = new();
    protected abstract string GeneratorClassPrefix { get; }

    public bool StartCreation(ClassInfo classInfo)
    {
        if (classInfo.IsAbstract())
            return false;
        NamespaceProvider.Usings = new System.Collections.Generic.List<string>()
        {
            " System",
            " VYaml.Parser",
            " VYaml.Emitter",
            " VYaml.Serialization",
            " System.Text",
            " Stride.Core",
        };
        classInfo.TypeName = GetIdentifierName(classInfo.TypeSyntax);
        classInfo.SerializerName = GeneratorClassPrefix + classInfo.TypeName;

        AttributeSyntax compilerGeneratedAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Runtime.CompilerServices.CompilerGenerated"));

        classInfo.SerializerSyntax = SyntaxFactory.ClassDeclaration(classInfo.SerializerName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(compilerGeneratedAttribute));
        NamespaceDeclarationSyntax normalNamespace = NamespaceProvider.GetNamespaceFromSyntaxNode(classInfo);
        if (normalNamespace == null)
            return false;
        normalNamespace = NamespaceProvider.AddUsingDirectivess(normalNamespace, classInfo.ExecutionContext);
        classInfo.SerializerSyntax = CreateGenerator(classInfo);

        classInfo.SerializerSyntax = AddInterfaces(classInfo.SerializerSyntax, classInfo.TypeName, classInfo);
        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                                      .AddMembers(normalNamespace
                                      .AddMembers(classInfo.SerializerSyntax));

        string sourceText = compilationUnit
            .NormalizeWhitespace()
            .ToFullString();

        string namespaceName = normalNamespace.Name.ToString();
        classInfo.ExecutionContext.AddSource($"{classInfo.SerializerName}_{namespaceName}.g.cs", sourceText);
        return true;
    }
    protected abstract ClassDeclarationSyntax CreateGenerator(ClassInfo classInfo);

    protected string GetIdentifierName(TypeDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Identifier.ValueText;
    }
    protected T AddMember<T>(T context, string newMember)
        where T : TypeDeclarationSyntax
    {
        context = (T)context.AddMembers(SyntaxFactory.ParseMemberDeclaration(newMember));
        return context;
    }
    protected ClassDeclarationSyntax AddInterfaces(ClassDeclarationSyntax partialClass, string className, ClassInfo classInfo)
    {
        classInfo.SerializerSyntax = classInfo.SerializerSyntax.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IRegisterYamlFormatter")));
        return AddYamlFormatterInterface(classInfo);
    }
    protected ClassDeclarationSyntax AddYamlFormatterInterface(ClassInfo classInfo)
    {
        if (classInfo.Generics != null && classInfo.Generics.Parameters.Count > 0)
        {
            var typeParameterList = SyntaxFactory.TypeParameterList(classInfo.Generics.Parameters);

            classInfo.SerializerSyntax = classInfo.SerializerSyntax.WithTypeParameterList(typeParameterList);

            var genericTypeName = SyntaxFactory.ParseTypeName($"{classInfo.TypeName}<{string.Join(", ", classInfo.Generics.Parameters)}>");
            return classInfo.SerializerSyntax.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlFormatter<{genericTypeName}?>")));
        }
        return classInfo.SerializerSyntax.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlFormatter<{classInfo.TypeName}?>"))); ;
    }

}
