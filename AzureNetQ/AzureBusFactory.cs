namespace AzureNetQ
{
    public static class AzureBusFactory
    {
        public static IBus CreateBus()
        {
            return CreateBus(new AzureNetQSettings());
        }

        public static IBus CreateBus(AzureNetQSettings settings)
        {
            return new AzureBus(
                settings.Logger(),
                settings.Conventions(),
                settings.Rpc(),
                settings.SendAndReceive(),
                settings.AzureAdvancedBus.Value,
                settings.ConnectionConfiguration());
        }
    }
}
