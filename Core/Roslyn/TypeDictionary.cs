using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.Core.Roslyn;
class TypeDictionary<TValue>
{
    private readonly Dictionary<TypeKey, TValue> dictionary = new Dictionary<TypeKey, TValue>();

    public void Add(Type type, TValue value)
    {
        dictionary[new TypeKey(type)] = value;
    }

    public TValue FindAssignableType(Type searchType)
    {
        TValue value;
        return dictionary.TryGetValue(new TypeKey(searchType), out value) ? value : default(TValue);
    }

}

class TypeKey
{
    private readonly Type type;

    public TypeKey(Type type)
    {
        this.type = type;
    }

    public override int GetHashCode()
    {
        // Use the generic type definition's hash code
        return type.IsGenericType ? type.GetGenericTypeDefinition().GetHashCode() : type.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is TypeKey otherKey)
        {
            Type otherType = otherKey.type;
            Type thisGenericType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            Type otherGenericType = otherType.IsGenericType ? otherType.GetGenericTypeDefinition() : otherType;

            return thisGenericType == otherGenericType;
        }
        return false;
    }
}