namespace AzureNetQ.FluentConfiguration
{
    public interface IRequestConfiguration
    {
        int? Affinity { get; }

        IRequestConfiguration WithAffinity(int? affinity);
    }

    public class RequestConfiguration : IRequestConfiguration
    {
        public int? Affinity { get; private set; }

        public IRequestConfiguration WithAffinity(int? affinity)
        {
            this.Affinity = affinity;
            return this;
        }
    }
}