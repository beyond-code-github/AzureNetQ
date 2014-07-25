namespace AzureNetQ
{
    using System;

    public interface IAzureNetQLogger
    {
        void DebugWrite(string format, params object[] args);

        void InfoWrite(string format, params object[] args);

        void ErrorWrite(string format, params object[] args);

        void ErrorWrite(Exception exception);
    }
}