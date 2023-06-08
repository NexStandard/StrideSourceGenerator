using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace StrideSourceGenerator;
public class NamespaceProvider
{
    public static NamespaceDeclarationSyntax GetNamespaceFromSyntaxNode(SyntaxNode s)
    {
        NamespaceProvider provider = new();
        while (s != null)
        {
            if (s is BaseNamespaceDeclarationSyntax baseNamespace)
            {
                return provider.ConvertToNormalNamespace(baseNamespace);
            }
            s = s.Parent;
        }

        return null;
    }
    private NamespaceDeclarationSyntax ConvertToNormalNamespace(BaseNamespaceDeclarationSyntax namespaceDeclaration)
    {
        if (namespaceDeclaration == null) return null;
        if(namespaceDeclaration is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
        {
            return CreateFileScopedNamespace(fileScopedNamespace);
        }
        if(namespaceDeclaration is NamespaceDeclarationSyntax normalNamespace)
        {
            return CreateNormalNamespace(normalNamespace);
        }
        return null;
    }
    private NamespaceDeclarationSyntax CreateFileScopedNamespace(FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
    {
        var name = fileScopedNamespace.Name;
        return SyntaxFactory.NamespaceDeclaration(name);
    }
    private NamespaceDeclarationSyntax CreateNormalNamespace(NamespaceDeclarationSyntax normalNamespace)
    {
        var name = normalNamespace.Name;
        return SyntaxFactory.NamespaceDeclaration(name);
    }
}
