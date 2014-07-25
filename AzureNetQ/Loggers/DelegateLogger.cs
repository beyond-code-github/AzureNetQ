namespace AzureNetQ.Loggers
{
    using System;

    public class DelegateLogger : IAzureNetQLogger
    {
        public DelegateLogger()
        {
            this.DefaultWrite = (s, o) => { };
        }

        public Action<string, object[]> DefaultWrite { get; set; }

        public Action<string, object[]> DebugWriteDelegate { get; set; }

        public Action<string, object[]> InfoWriteDelegate { get; set; }

        public Action<string, object[]> ErrorWriteDelegate { get; set; }

        public void DebugWrite(string format, params object[] args)
        {
            if (this.DebugWriteDelegate == null)
            {
                this.DefaultWrite(format, args);
            }
            else
            {
                this.DebugWriteDelegate(format, args);
            }
        }

        public void InfoWrite(string format, params object[] args)
        {
            if (this.InfoWriteDelegate == null)
            {
                this.DefaultWrite(format, args);
            }
            else
            {
                this.InfoWriteDelegate(format, args);
            }
        }

        public void ErrorWrite(string format, params object[] args)
        {
            if (this.ErrorWriteDelegate == null)
            {
                this.DefaultWrite(format, args);
            }
            else
            {
                this.ErrorWriteDelegate(format, args);
            }
        }

        public void ErrorWrite(Exception exception)
        {
            if (this.ErrorWriteDelegate == null)
            {
                this.DefaultWrite(exception.ToString(), new object[0]);
            }
            else
            {
                this.ErrorWriteDelegate(exception.ToString(), new object[0]);
            }
        }
    }
}