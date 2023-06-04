using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace StrideSourceGenerator;
internal class WriterFactory
{
    public string Test(IEnumerable<PropertyDeclarationSyntax> properties, string className, List<IPropertySymbol> symbols)
    {
        return WriteToDictionaryTemplate(properties, className,symbols);

    }
    public string WriteToDictionaryTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className,IEnumerable<IPropertySymbol> symbols)
    {
 
        StringBuilder sb = new StringBuilder();
            sb.Append("if(objToSerialize is not null) { ");
        foreach(var inheritedProperty in symbols)
        {
            var propertyname = inheritedProperty.Name;
            var type = inheritedProperty.Type.Name;
            if (SimpleTypes.Contains(type))
            {
                sb.Append( CreateSimpleType(propertyname, type));
            }
            else if(inheritedProperty.Type.TypeKind == TypeKind.Class)
            {
                return $"propertiesDictionary[\"{propertyname}\"] = GeneratedSerializer{type}.WriteToDictionary(objToSerialize.{propertyname});";
            }
        }
        
        foreach (var property in properties)
        {
            var propertyName = property.Identifier.Text;
            var type = property.Type.ToString();
            if (SimpleTypes.Contains(type))
            {
                sb.Append(CreateSimpleType(propertyName, type));
            }
            else if (property.Type is IdentifierNameSyntax classIdentifier)
            {
                sb.Append($"propertiesDictionary[\"{propertyName}\"] = GeneratedSerializer{classIdentifier}.WriteToDictionary(objToSerialize.{propertyName});");
            }
        }
        sb.Append("}");
        return $$"""
        public static Dictionary<object,object> WriteToDictionary({{className}} objToSerialize)
        {
        Dictionary<object,object> objectAsDictionary = new Dictionary<object,object>();
        objectAsDictionary["$Type"] = typeof({{className}}).AssemblyQualifiedName;
        var propertiesDictionary = new Dictionary<object,object>();
        objectAsDictionary["$Attributes"] = propertiesDictionary;

        {{sb}}

        return objectAsDictionary;
        }
        """;
        
    }

    private List<string> SimpleTypes = new ()
    {
        "int",
        "Int32",
        "string",
        "String",
        "float",
        "double",
        "long",
        "UInt64",
        "Int64",
        "byte",
        "Byte"
    };
    private string CreateSimpleType(string name, string type)
    {
        return $"propertiesDictionary[\"{name}\"] = objToSerialize.{name};";
    }
}
