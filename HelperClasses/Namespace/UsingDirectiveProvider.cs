using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Threading;

namespace StrideSourceGenerator.HelperClasses.Namespace;
internal class UsingDirectiveProvider
{
    // TODO: replace links with real error pages.
    List<UsingDirective> allowedAttributes = new List<UsingDirective>()
    {
        new UsingDirective() { 
            Name = " YamlDotNet.RepresentationModel",
            MetadataName = "YamlDotNet.RepresentationModel",
            NugetReference = "<ProjectReference Include=\"..\\NexStandard\\YamlDotNet\\YamlDotNet\\YamlDotNet.csproj\"/>",
            HelpLink = "https://stride-docs-test.azurewebsites.net/latest"
        }
        //" Stride.Core.YamlDotNet",
        
    };
    public NamespaceDeclarationSyntax AddUsingDirectives(NamespaceDeclarationSyntax normalNamespace,ClassDeclarationSyntax classContext, GeneratorExecutionContext context,string className)
    {

        var compilation = context.Compilation;
        foreach (var allowedUsing in  allowedAttributes)
        {
                normalNamespace = normalNamespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(allowedUsing.Name)));
        }
        return normalNamespace;
    }

    // TODO: doesnt work as idk how to validate if (the reference is validated.
    // Also its unknown how to trigger VS to install a missing nuget package.
    private static DiagnosticDescriptor UsingDirectiveNotAvailable(UsingDirective directive)
    {
        return  new DiagnosticDescriptor(
            id: "SYSG002",
            title: "Missing NuGet Package",
            messageFormat: $"The '{directive.MetadataName}' NuGet package is missing. Click here to add it as a solution.",
            category: "Stride.Yaml.CompilerServices",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: $"The Namespace {directive.MetadataName} is not referenced in the project.\nExpected reference was {directive.MetadataName} but it was not found.\nThe namespace must be included in the csproj file and be referenced.\nOne option to include it would be with the nuget \"{directive.NugetReference}\"",
            helpLinkUri: directive.HelpLink);
    }
}

internal class UsingDirective
{
    public string Name { get; set; }
    public string MetadataName { get; set; }
    public string NugetReference { get; set; }
    public string HelpLink { get; set; }
}