using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StrideSourceGenerator.API;

namespace StrideSourceGenerator.Core.Templates;

internal class TypeTemplateProvider : ITemplateProvider
{
    public MemberDeclarationSyntax GetTemplate(string className)
    {
        return SyntaxFactory.ParseMemberDeclaration($"public Type IdentifierType {{ get; }} = typeof({className});");
    }
}
