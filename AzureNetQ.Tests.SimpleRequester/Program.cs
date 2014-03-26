using System;
using System.Threading;
using AzureNetQ.Loggers;

namespace AzureNetQ.Tests.SimpleRequester
{
    using AzureNetQ.Tests.Messages;

    class Program
    {
        private static readonly IBus bus =
            AzureBusFactory.CreateBus(
                new AzureNetQSettings
                    {
                        Logger = () => new NoDebugLogger(),
                        ConnectionConfiguration = () => new ConnectionConfiguration
                        {
                            PrefetchCount = 200,
                            MaxConcurrentCalls = 100,
                            BatchingInterval = TimeSpan.FromMilliseconds(50)
                        }
                    });
        
        private static long count = 0;

        private static readonly ILatencyRecorder latencyRecorder = new LatencyRecorder();
        
        private const int publishIntervalMilliseconds = 20;

        static void Main(string[] args)
        {
            var timer = new Timer(OnTimer, null, publishIntervalMilliseconds, publishIntervalMilliseconds);

            Console.Out.WriteLine("Timer running, ctrl-C to end");

            Console.CancelKeyPress += (source, cancelKeyPressArgs) =>
            {
                Console.Out.WriteLine("Shutting down");

                timer.Dispose();
                bus.Dispose();
                latencyRecorder.Dispose();

                Console.WriteLine("Shut down complete");
            };

            Thread.Sleep(Timeout.Infinite);
        }

        private static readonly object requestLock = new object();

        static void OnTimer(object state)
        {
            try
            {
                lock (requestLock)
                {
                    Console.WriteLine(string.Format("Sending {0}", count));
                    bus.RequestAsync<TestAsyncRequestMessage, TestAsyncResponseMessage>(
                        new TestAsyncRequestMessage
                        {
                            Id = count,
                            Text = string.Format("Hello from client number: {0}! ", count)
                        }).ContinueWith(
                            t =>
                                {
                                    if (t.IsFaulted && t.Exception != null)
                                    {
                                        foreach (var exception in t.Exception.InnerExceptions)
                                        {
                                            Console.WriteLine("Exception thrown by Response: {0}", exception.Message);
                                        }

                                        return;
                                    }

                                    ResponseHandler(t.Result);
                                });

                    latencyRecorder.RegisterRequest(count);
                    count++;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception thrown by Publish: {0}", exception.Message);
            }
        }
        
        static void ResponseHandler(TestAsyncResponseMessage response)
        {
            Console.WriteLine("Response: {0}", response.Text);
            latencyRecorder.RegisterResponse(response.Id);
        }
    }

    public class NoDebugLogger : IAzureNetQLogger
    {
        private readonly ConsoleLogger consoleLogger = new ConsoleLogger();

        public void DebugWrite(string format, params object[] args)
        {
            // do nothing
        }

        public void InfoWrite(string format, params object[] args)
        {
            // do nothing
        }

        public void ErrorWrite(string format, params object[] args)
        {
            consoleLogger.ErrorWrite(format, args);
        }

        public void ErrorWrite(Exception exception)
        {
            consoleLogger.ErrorWrite(exception);
        }
    }
}
