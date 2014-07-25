namespace AzureNetQ.Loggers
{
    using System;

    /// <summary>
    /// noop logger
    /// </summary>
    public class NullLogger : IAzureNetQLogger
    {
        public void DebugWrite(string format, params object[] args)
        {
        }

        public void InfoWrite(string format, params object[] args)
        {
        }

        public void ErrorWrite(string format, params object[] args)
        {
        }

        public void ErrorWrite(Exception exception)
        {
        }
    }
}