namespace AzureNetQ
{
    public interface ISerializer
    {
        byte[] MessageToBytes<T>(T message) where T : class;

        string MessageToString<T>(T message) where T : class;

        T BytesToMessage<T>(byte[] bytes);

        T StringToMessage<T>(string content);

        object BytesToMessage(string typeName, byte[] bytes);

        object StringToMessage(string typeName, string content);
    }
}
