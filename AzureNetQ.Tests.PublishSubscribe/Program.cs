namespace AzureNetQ.Tests.PublishSubscribe
{
    using System;
    using System.Threading.Tasks;

    using AzureNetQ.NonGeneric;

    public class Program
    {
        private IBus bus;

        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            this.bus = AzureBusFactory.CreateBus("Endpoint=sb://servicebus/ServiceBusDefaultNamespace;StsEndpoint=https://servicebus:10355/ServiceBusDefaultNamespace;RuntimePort=10354;ManagementPort=10355");

            this.bus.SubscribeAsync(
                typeof(TestMessage),
                obj => Task.Factory.StartNew(() => Console.WriteLine("Handler Received message: {0}", ((TestMessage)obj).Text)));
                
            Console.WriteLine("Type a message or 'q' to quit.");

            string text = null;
            while ((text = Console.ReadLine()) != "q")
            {
                this.bus.Publish(
                        typeof(TestMessage),
                        new TestMessage
                        {
                            Text = text
                        });
            }

            this.bus.Dispose();
        }
    }

    [Serializable]
    public class TestMessage
    {
        public string Text { get; set; }
    }
}
