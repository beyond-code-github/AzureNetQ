using System;
using System.Threading;

namespace AzureNetQ.Tests.Performance.Consumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new NoDebugLogger();

            throw new NotImplementedException();

            //var bus = RabbitHutch.CreateBus("host=localhost;product=consumer", 
            //    x => x.Register<IAzureNetQLogger>(_ => logger));

            //int messageCount = 0;
            //var timer = new Timer(state =>
            //{
            //    Console.Out.WriteLine("messages per second = {0}", messageCount);
            //    Interlocked.Exchange(ref messageCount, 0);
            //}, null, 1000, 1000);

            //bus.Subscribe<TestPerformanceMessage>("consumer", message => Interlocked.Increment(ref messageCount));

            //Console.CancelKeyPress += (source, cancelKeyPressArgs) =>
            //{
            //    Console.Out.WriteLine("Shutting down");
            //    bus.Dispose();
            //    timer.Dispose();
            //    Console.WriteLine("Shut down complete");
            //};

            //Thread.Sleep(Timeout.Infinite);
        }
    }

    public class NoDebugLogger : IAzureNetQLogger
    {
        public void DebugWrite(string format, params object[] args)
        {

        }

        public void InfoWrite(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void ErrorWrite(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void ErrorWrite(Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
