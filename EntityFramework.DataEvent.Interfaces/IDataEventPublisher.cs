namespace EntityFramework.DataEvent.Interfaces
{
    public interface IDataEventPublisher
    {

        public bool PublishObjectUpdates { get; set; }
        public bool PublishObjectCreated { get; set; }
        public bool PublishObjectDeleted { get; set; }

        public Task OnTransactionCompleted();
        public Task OnTransactionFailed();
        public Task OnDataItemUpdated(object data, string correlationId);
        public Task OnDataItemDeleted(object data, string correlationId);
        public Task OnDataItemCreated(object data, string correlationId);
    }

    public enum DataEventType
    {
        Created, Updated, Deleted, Accessed, Exception, Notification
    }
}