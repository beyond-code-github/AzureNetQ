using AzureNetQ.Tests.Messages;
using System;

namespace AzureNetQ.Tests.SendReceive
{
    class Program
    {
        private static string SendReceiveQueue = "azurenetq.tests.sendreceive";
        private static string AuditQueue = "azurenetq.tests.sendreceive.audit";
        
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private IBus bus;

        private void Run()
        {
            bus = AzureBusFactory.CreateBus();

            bus.Receive(SendReceiveQueue, handlers =>
            {
                handlers
                    .Add<TestSendMessage>(msg => Console.WriteLine("Handler 1 Received message: {0}", msg.Text))
                    .Add<TestSendMessage2>(msg => Console.WriteLine("Handler 2 Received message: {0}", msg.Text));
            });

            bus.Receive<TestSendMessageBase>(AuditQueue, msg => Console.WriteLine("Auditor Received message: {0}", msg.Text));

            Console.WriteLine("Type a message or 'q' to quit.");

            string text = null;
            while ((text = Console.ReadLine()) != "q")
            {
                foreach (var queue in new[] { SendReceiveQueue, AuditQueue })
                {
                    bus.Send(queue, new TestSendMessage
                    {
                        Text = text
                    });

                    bus.Send(queue, new TestSendMessage2
                    {
                        Text = text
                    });
                }
            }

            bus.Dispose();
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
