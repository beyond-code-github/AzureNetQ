namespace AzureNetQ.Tests.Messages
{
    using System;

    [Serializable]
    public class TestRequestMessage
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public bool CausesExceptionInServer { get; set; }
        public string ExceptionInServerMessage { get; set; }
        public bool CausesServerToTakeALongTimeToRespond { get; set; }
    }

    [Serializable]
    public class TestResponseMessage
    {
        public long Id { get; set; }
        public string Text { get; set; }
    }

    [Serializable]
    public class TestAsyncRequestMessage
    {
        public long Id { get; set; }
        public string Text { get; set; }
    }

    [Serializable]
    public class TestAsyncResponseMessage
    {
        public long Id { get; set; }
        public string Text { get; set; }
    }
}
