namespace EntityFramework.DataEvent
{


    public record DataEventMessage
    {
        public DataEventMessage(object data, string subject, string dataType,string correlationId, object RequestContext)
        {
            this.data = data;
            this.subject = subject;
            this.dataType = dataType;
            this.requestContext = RequestContext;
            this.correlationid = correlationId;
            this.eventType = eventType;
        }

        public object data { get; private set; }
        public string subject { get; }
        public string dataType { get; }
        public object requestContext { get; }
        public string correlationid { get; }
        public DataEventType eventType { get; }
    }
    public enum DataEventType
    {
        Created,Updated,Deleted,Accessed,Exception,Notification
    }



}