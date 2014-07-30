namespace AzureNetQ.Loggers
{
    using System;

    public class ConsoleLogger : IAzureNetQLogger
    {
        public ConsoleLogger()
        {
            this.Debug = true;
            this.Info = true;
            this.Error = true;
        }

        public bool Debug { get; set; }

        public bool Info { get; set; }

        public bool Error { get; set; }

        public void DebugWrite(string format, params object[] args)
        {
            if (!this.Debug)
            {
                return;
            }

            this.SafeConsoleWrite(DateTime.UtcNow.ToString("dd-MM-yyyy hh:mm:ss") + " DEBUG: " + format, args);
        }

        public void InfoWrite(string format, params object[] args)
        {
            if (!this.Info)
            {
                return;
            }

            this.SafeConsoleWrite(DateTime.UtcNow.ToString("dd-MM-yyyy hh:mm:ss") + " INFO: " + format, args);
        }

        public void ErrorWrite(string format, params object[] args)
        {
            if (!this.Error)
            {
                return;
            }

            this.SafeConsoleWrite(DateTime.UtcNow.ToString("dd-MM-yyyy hh:mm:ss") + " ERROR: " + format, args);
        }

        public void SafeConsoleWrite(string format, params object[] args)
        {
            // even a zero length args paramter causes WriteLine to interpret 'format' as
            // a format string. Rather than escape JSON, better to check the intention of 
            // the caller.
            if (args.Length == 0)
            {
                Console.WriteLine(format);
            }
            else
            {
                Console.WriteLine(format, args);
            }
        }

        public void ErrorWrite(Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}