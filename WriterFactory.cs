using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace StrideSourceGenerator;
internal class WriterFactory
{
    public string Test(IEnumerable<PropertyDeclarationSyntax> properties, string className)
    {
        return WriteToDictionaryTemplate(properties, className);

    }
    public string WriteToDictionaryTemplate(IEnumerable<PropertyDeclarationSyntax> properties, string className)
    {
 
        StringBuilder sb = new StringBuilder();
            sb.Append("if(objToSerialize is not null) { ");
        foreach (var property in properties)
        {
            var propertyName = property.Identifier.Text;

            if (property.Type.ToString() == "int" || property.Type.ToString() == "Int32")
            {
                // TODO: This is for reading it
                // sb.Append($"objectAsDictionary[\"{propertyName}\"] = Int32.Parse(objToSerialize.{propertyName});");
                sb.Append($"propertiesDictionary[\"{propertyName}\"] = objToSerialize.{propertyName};");
            }
            else if(property.Type.ToString() == "string" || property.Type.ToString() == "String")
            {
                sb.Append($"propertiesDictionary[\"{propertyName}\"] = objToSerialize.{propertyName};");
            }
            else if (property.Type is IdentifierNameSyntax classIdentifier)
            {
                sb.Append($"propertiesDictionary[\"{propertyName}\"] = GeneratedSerializer{classIdentifier}.WriteToDictionary(objToSerialize.{propertyName});");
            }
            else
            {
                
            }
            
        }
        sb.Append("}");
        return $$"""
        public static Dictionary<object,object> WriteToDictionary({{className}} objToSerialize)
        {
        Dictionary<object,object> objectAsDictionary = new Dictionary<object,object>();
        objectAsDictionary["!Type"] = typeof({{className}}).AssemblyQualifiedName;
        var propertiesDictionary = new Dictionary<object,object>();
        objectAsDictionary["!Attributes"] = propertiesDictionary;

        {{sb}}

        return objectAsDictionary;
        }
        """;
        
    }
}
