using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using StrideSourceGenerator.Core;
using System.Linq;

namespace StrideSourceGenerator;

public class NexSyntaxReceiver : ISyntaxReceiver
{
    TypeAttributeFinder _typeFinder = new();
    public List<TypeDeclarationSyntax> TypeDeclarations { get; private set; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        TypeDeclarationSyntax result = _typeFinder.FindAttribute(syntaxNode);

        if (result != null && (HasDataContractAttribute(result) || HasStrideCoreDataContractAttribute(result)))
        {
            TypeDeclarations.Add(result);
        }
    }
    private bool HasDataContractAttribute(TypeDeclarationSyntax typeDeclaration)
    {
        // Check if the type declaration has the [DataContract] attribute
        return typeDeclaration.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute => attribute.Name.ToString() == "DataContract");
    }

    private bool HasStrideCoreDataContractAttribute(TypeDeclarationSyntax typeDeclaration)
    {
        // Check if the type declaration has the [Stride.Core.DataContract] attribute
        return typeDeclaration.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute => attribute.Name.ToString() == "DataContract" && attribute.Name.ToString() == "Stride.Core.DataContract");
    }
}
