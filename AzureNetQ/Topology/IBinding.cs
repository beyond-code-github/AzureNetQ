namespace AzureNetQ.Topology
{
    public interface IBinding
    {
        IBindable Bindable { get; }
        IExchange Exchange { get; }
        string RoutingKey { get; }
    }
}