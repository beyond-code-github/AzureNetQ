namespace AzureNetQ.Tests.SendReceive
{
    using System;

    public class Program
    {
        private const string SendReceiveQueue = "azurenetq.tests.sendreceive";

        private const string AuditQueue = "azurenetq.tests.sendreceive.audit";

        private IBus bus;

        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            this.bus = AzureBusFactory.CreateBus();

            this.bus.Receive(
                SendReceiveQueue,
                handlers => handlers
                                .Add<TestSendMessage>(msg => Console.WriteLine("Handler 1 Received message: {0}", msg.Text))
                                .Add<TestSendMessage2>(msg => Console.WriteLine("Handler 2 Received message: {0}", msg.Text)));

            this.bus.Receive<TestSendMessageBase>(AuditQueue, msg => Console.WriteLine("Auditor Received message: {0}", msg.Text));

            Console.WriteLine("Type a message or 'q' to quit.");

            string text = null;
            while ((text = Console.ReadLine()) != "q")
            {
                foreach (var queue in new[] { SendReceiveQueue, AuditQueue })
                {
                    this.bus.Send(
                        queue,
                        new TestSendMessage
                            {
                                Text = text
                    });

                    this.bus.Send(
                        queue,
                        new TestSendMessage2
                            {
                                Text = text
                    });
                }
            }

            this.bus.Dispose();
        }
    }

    [Serializable]
    public class TestSendMessageBase
    {
        public string Text { get; set; }
    }

    [Serializable]
    public class TestSendMessage : TestSendMessageBase
    {
    }

    [Serializable]
    public class TestSendMessage2 : TestSendMessageBase
    {
    }
}
