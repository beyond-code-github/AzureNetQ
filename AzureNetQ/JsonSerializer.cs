namespace AzureNetQ
{
    using System.Text;

    using Newtonsoft.Json;

    public class JsonSerializer : ISerializer
    {
        private readonly ITypeNameSerializer typeNameSerializer;

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public JsonSerializer(ITypeNameSerializer typeNameSerializer)
        {
            Preconditions.CheckNotNull(typeNameSerializer, "typeNameSerializer");

            this.typeNameSerializer = typeNameSerializer;
        }

        public byte[] MessageToBytes<T>(T message) where T : class
        {
            Preconditions.CheckNotNull(message, "message");
            return Encoding.UTF8.GetBytes(this.MessageToString(message));
        }

        public string MessageToString<T>(T message) where T : class
        {
            Preconditions.CheckNotNull(message, "message");
            return JsonConvert.SerializeObject(message, this.serializerSettings);
        }

        public T BytesToMessage<T>(byte[] bytes)
        {
            Preconditions.CheckNotNull(bytes, "bytes");
            return this.StringToMessage<T>(Encoding.UTF8.GetString(bytes));
        }

        public T StringToMessage<T>(string content)
        {
            Preconditions.CheckNotNull(content, "content");
            return JsonConvert.DeserializeObject<T>(content, this.serializerSettings);
        }

        public object BytesToMessage(string typeName, byte[] bytes)
        {
            Preconditions.CheckNotNull(typeName, "typeName");
            Preconditions.CheckNotNull(bytes, "bytes");

            var type = this.typeNameSerializer.DeSerialize(typeName);

            return this.StringToMessage(typeName, Encoding.UTF8.GetString(bytes));
        }

        public object StringToMessage(string typeName, string content)
        {
            Preconditions.CheckNotNull(typeName, "typeName");
            Preconditions.CheckNotNull(content, "content");

            var type = this.typeNameSerializer.DeSerialize(typeName);

            return JsonConvert.DeserializeObject(content, type, this.serializerSettings);
        }
    }
}
