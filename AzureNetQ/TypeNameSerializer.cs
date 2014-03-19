using System;

namespace AzureNetQ
{
    public interface ITypeNameSerializer
    {
        string Serialize(Type type);
        Type DeSerialize(string typeName);
    }

    public class TypeNameSerializer : ITypeNameSerializer
    {
        public Type DeSerialize(string typeName)
        {
            var nameParts = typeName.Split(':');
            if (nameParts.Length != 2)
            {
                throw new AzureNetQException(
                    "type name {0}, is not a valid AzureNetQ type name. Expected Type:Assembly", 
                    typeName);
            }
            var type = Type.GetType(nameParts[0] + ", " + nameParts[1]);
            if (type == null)
            {
                throw new AzureNetQException(
                    "Cannot find type {0}",
                    typeName);
            }
            return type;
        }

        public string Serialize(Type type)
        {
            Preconditions.CheckNotNull(type, "type");
            var typeName = type.FullName + ":" + type.Assembly.GetName().Name;
            if (typeName.Length > 255)
            {
                throw new AzureNetQException("The serialized name of type '{0}' exceeds the AMQP" + 
                    "maximum short string lengh of 255 characters.",
                    type.Name);
            }
            return typeName;
        }
    }
}