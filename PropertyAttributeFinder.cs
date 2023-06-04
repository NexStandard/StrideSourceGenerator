using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace StrideSourceGenerator;
internal class PropertyAttributeFinder
{
    List<String> allowedAttributes = new List<string>()
    {
       // typeof(Stride.Core.DataMember),
       "DataMember",
       "DataMemberAttribute",
        "System.Runtime.Serialization.DataMemberAttribute",
        "System.Runtime.Serialization.DataMember"
    };
    public IEnumerable<PropertyDeclarationSyntax> FilterProperties(IEnumerable<PropertyDeclarationSyntax> properties)
    {
        return properties.Where(x =>
        {
            var attributes = x.AttributeLists.SelectMany(b => b.Attributes);
            foreach(var attribute in attributes)
            {
                if(allowedAttributes.Contains(attribute.Name.ToString()))
                    return true;
            }
            return false;
        });
    }

}
