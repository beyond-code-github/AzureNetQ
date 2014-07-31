namespace AzureNetQ.Tests.SimpleRequester
{
    using System;
    using System.Threading;

    using AzureNetQ.Loggers;
    using AzureNetQ.Tests.Messages;

    public class Program
    {
        private const int PublishIntervalMilliseconds = 20;

        private static readonly object RequestLock = new object();

        private static readonly IBus Bus =
            AzureBusFactory.CreateBus(
                new AzureNetQSettings
                    {
                        Logger = () => new NoDebugLogger(),
                        ConnectionConfiguration = new ConnectionConfiguration
                        {
                            PrefetchCount = 200,
                            MaxConcurrentCalls = 100,
                            BatchingInterval = TimeSpan.FromMilliseconds(50)
                        }
                    });

        private static readonly ILatencyRecorder LatencyRecorder = new LatencyRecorder();

        private static long count;

        public static void Main(string[] args)
        {
            var timer = new Timer(OnTimer, null, PublishIntervalMilliseconds, PublishIntervalMilliseconds);

            Console.Out.WriteLine("Timer running, ctrl-C to end");

            Console.CancelKeyPress += (source, cancelKeyPressArgs) =>
            {
                Console.Out.WriteLine("Shutting down");

                timer.Dispose();
                Bus.Dispose();
                LatencyRecorder.Dispose();

                Console.WriteLine("Shut down complete");
            };

            Thread.Sleep(Timeout.Infinite);
        }

        public static void OnTimer(object state)
        {
            try
            {
                lock (RequestLock)
                {
                    Console.WriteLine("Sending {0}", count);
                    Bus.RequestAsync<TestAsyncRequestMessage, TestAsyncResponseMessage>(
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

                    LatencyRecorder.RegisterRequest(count);
                    count++;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception thrown by Publish: {0}", exception.Message);
            }
        }
        
        public static void ResponseHandler(TestAsyncResponseMessage response)
        {
            Console.WriteLine("Response: {0}", response.Text);
            LatencyRecorder.RegisterResponse(response.Id);
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
            this.consoleLogger.ErrorWrite(format, args);
        }

        public void ErrorWrite(Exception exception)
        {
            this.consoleLogger.ErrorWrite(exception);
        }
    }
}
