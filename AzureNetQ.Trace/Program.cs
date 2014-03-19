using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

using CommandLine;
using CommandLine.Text;

namespace AzureNetQ.Trace
{
    class Program
    {
        private const string traceExchange = "amq.rabbitmq.trace";
        private const string publishRoutingKey = "publish.#";
        private const string deliverRoutingKey = "deliver.#";
        private static readonly CancellationTokenSource tokenSource = 
            new CancellationTokenSource();

        private static readonly Options options = new Options();
        private static CSVFile csvFile;


        static void Main(string[] args)
        {

            
            Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    tokenSource.Cancel();
                };

            if (Parser.Default.ParseArguments(args, options))
            {

                if (options.csvoutput != null)
                {
                    //Create CSV file and write header row.
                    csvFile = new CSVFile(options.csvoutput);

                    var columnlist = new List<string>
                        {
                            "Message#", 
                            "Date Time", 
                            "Routing Key", 
                            "Exchange", 
                            "Body"
                        };

                    csvFile.WriteRow(columnlist);

                }


                var connectionString = options.AMQP;

                Console.WriteLine("Trace is running. Ctrl-C to exit");

                HandleDelivery();
                try
                {

                    using (ConnectAndSubscribe(connectionString))
                    {
                        tokenSource.Token.WaitHandle.WaitOne();
                    }

                    Console.WriteLine("Shutdown");
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.Message);
                }
                
            }
        }

        static void HandleDelivery()
        {
            int msgCount = 0;
            new Thread(() =>
                {
                    throw new NotImplementedException();
                    //try
                    //{
                    //    foreach (var deliverEventArgs in deliveryQueue.GetConsumingEnumerable(tokenSource.Token))
                    //    {
                    //        HandleDelivery(deliverEventArgs,msgCount++);
                    //    }
                    //}
                    //// deliveryQueue has been disposed so do nothing
                    //catch (OperationCanceledException)
                    //{}
                    //catch (ObjectDisposedException)
                    //{}
                })
                {
                    Name = "AzureNetQ.Trace - delivery."
                }.Start();

        
        }

        static IDisposable ConnectAndSubscribe(string connectionString)
        {
            throw new NotImplementedException();

            //var connectionFactory = new ConnectionFactory
            //    {
            //        Uri = connectionString,
            //        ClientProperties = new Dictionary<string, object>
            //            {
            //                { "Client", "AzureNetQ.Trace" },
            //                { "Host", Environment.MachineName }
            //            },
            //        RequestedHeartbeat = 10
            //    };

            //var connection = connectionFactory.CreateConnection();
            //var disposable = new Disposable{ ToBeDisposed = connection };
            //connection.ConnectionShutdown += (connection1, reason) =>
            //    {
            //        if(!tokenSource.IsCancellationRequested)
            //        {
            //            Console.Out.WriteLine("\nConnection closed.\nReason {0}\nNow reconnecting", reason.ToString());
            //            disposable.ToBeDisposed = ConnectAndSubscribe(connectionString);
            //        }
            //    };

            //Subscribe(connection, traceExchange, publishRoutingKey);
            //Subscribe(connection, traceExchange, deliverRoutingKey);

            //return disposable;
        }
    }

    public class Disposable : IDisposable
    {
        public IDisposable ToBeDisposed { get; set; }

        public void Dispose()
        {
            if (ToBeDisposed != null)
            {
                ToBeDisposed.Dispose();
            }
        }
    }


    /// <summary>
    ///  Define command line options
    /// </summary>
    class Options
    {
        [Option('a', "amqp-connection-string", Required = false, DefaultValue = "amqp://localhost/", HelpText = "AMQP Connection string.")]
        public string AMQP { get; set; }

        [Option('q', "quiet", DefaultValue = false, HelpText = "Switch off verbose console output")]
        public bool quiet { get; set; }

        [Option('o', "output-csv", Required = false, HelpText = "CSV File name for output")]
        public string csvoutput { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
