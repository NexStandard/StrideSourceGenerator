using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.API;
using StrideSourceGenerator.Core.Methods;
using StrideSourceGenerator.Core.Properties;
using StrideSourceGenerator.Core.Roslyn;
using StrideSourceGenerator.Core.Templates;
namespace StrideSourceGenerator.Core.GeneratorCreators;

internal abstract class GeneratorBase<T>
    where T : TypeDeclarationSyntax
{
    protected ITemplateProvider TagTemplate = new TagTemplateProvider();
    protected ITemplateProvider TypeTemplate = new TypeTemplateProvider();
    protected PropertyAttributeFinder PropertyFinder { get; } = new();
    private NamespaceProvider<T> NamespaceProvider { get; } = new();
    protected SerializeMethodFactory writerFactory;
    protected DeserializeMethodFactory DeserializeMethodFactory = new();
    protected RegisterMethodFactory RegisterMethodFactory { get; } = new();
    protected abstract string GeneratorClassPrefix { get; }

    public bool StartCreation(ClassInfo<T> classInfo)
    {
        classInfo.TypeName = GetIdentifierName(classInfo.TypeSyntax);
        classInfo.SerializerName = GeneratorClassPrefix + classInfo.TypeName;
        
        var compilerGeneratedAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Runtime.CompilerServices.CompilerGenerated"));

        classInfo.SerializerSyntax = SyntaxFactory.ClassDeclaration(classInfo.SerializerName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(compilerGeneratedAttribute));
        NamespaceDeclarationSyntax normalNamespace = NamespaceProvider.GetNamespaceFromSyntaxNode(classInfo);
        if (normalNamespace == null)
            return false;
        normalNamespace = NamespaceProvider.AddUsingDirectivess(normalNamespace, classInfo.ExecutionContext);
        classInfo.SerializerSyntax = CreateGenerator(classInfo);

        classInfo.SerializerSyntax = AddInterfaces(classInfo.SerializerSyntax, classInfo.TypeName);
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
    protected abstract ClassDeclarationSyntax CreateGenerator(ClassInfo<T> classInfo);

    protected string GetIdentifierName(TypeDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Identifier.ValueText;
    }
    protected T AddMember(T context, string newMember)
    {
        context = (T)context.AddMembers(SyntaxFactory.ParseMemberDeclaration(newMember));
        return context;
    }
    protected ClassDeclarationSyntax AddInterfaces(ClassDeclarationSyntax partialClass, string className)
    {
        partialClass = partialClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IRegisterYamlFormatter")));
        return partialClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IYamlFormatter<{className}?>")));
    }

}
