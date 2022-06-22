using Azure.Messaging.ServiceBus;
using EntityFramework.DataEvent.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace EntityFramework.DataEvent.AzureServiceBus
{
    public class ServiceBusDataEventPublisher : IDataEventPublisher
    {
        public string TopicName { get; set; }
        public Object RequestContext { get; set; }
        public List<ServiceBusMessage> DataEvents { get; set; } = new List<ServiceBusMessage>();
        private string _ServieBusConnection { get; set; }
        public virtual bool PublishObjectUpdates { get; set; } = true;
        public virtual bool PublishObjectCreated { get; set; } = true;
        public virtual bool PublishObjectDeleted { get; set; } = true;



        public ServiceBusDataEventPublisher(IOptions<ServiceBusDataEventPublisherOptions> options, IConfiguration configuration)
        {
            var ops = options.Value;
            this.PublishObjectCreated = ops.PublishObjectCreated;
            this.PublishObjectDeleted = ops.PublishObjectDeleted;
            this.TopicName = ops.TopicName;
            this.PublishObjectUpdates = ops.PublishObjectUpdates;
            _ServieBusConnection = configuration?.GetValue<string>("ServiceBusDataEventPublisherConnection");// ?? "Endpoint=sb://botrespond-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=cuykC4iYay2bj2WhwXCABZ7ARCD1/D9gzMVg4Bto0v0=";
        }


        public async Task OnDataItemCreated(object data,string correlationId)
        {
            if (PublishObjectCreated)
            {
                IDataEventEntity? dataEvent = data as IDataEventEntity;
                if (dataEvent != null)
                {
                    var msgBody = JsonSerializer.Serialize(data);
                    
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(msgBody));
                    serviceBusMessage.CorrelationId = correlationId;
                    serviceBusMessage.ApplicationProperties.Add("requestContext", RequestContext);
                    serviceBusMessage.ContentType = "/" + dataEvent.GetDataType();
                    serviceBusMessage.Subject = "created";
                    DataEvents.Add(serviceBusMessage);
                }
            }
        }

        public async Task OnDataItemDeleted(object data, string correlationId)
        {
            if (PublishObjectCreated)
            {
                IDataEventEntity? dataEvent = data as IDataEventEntity;
                if (dataEvent != null)
                {
                    var msgBody = JsonSerializer.Serialize(data);
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(msgBody));
                    serviceBusMessage.ContentType = "/" + dataEvent.GetDataType();
                    serviceBusMessage.Subject = "deleted";
                    serviceBusMessage.CorrelationId = correlationId;
                    serviceBusMessage.ApplicationProperties.Add("requestContext", RequestContext);
                    DataEvents.Add(serviceBusMessage);
                }
            }
        }



        public async Task OnDataItemUpdated(object data, string correlationId)
        {
            if (PublishObjectUpdates)
            {
                IDataEventEntity? dataEvent = data as IDataEventEntity;
                if (dataEvent != null)
                {
                    var msgBody = JsonSerializer.Serialize(data);
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(msgBody));
                    serviceBusMessage.ContentType = "/" + dataEvent.GetDataType();
                    serviceBusMessage.Subject = "updated";
                    serviceBusMessage.CorrelationId = correlationId;
                    serviceBusMessage.ApplicationProperties.Add("requestContext", RequestContext);
                    DataEvents.Add(serviceBusMessage);
                }
            }
        }

        public async Task OnTransactionCompleted()
        {
            await using var client = new ServiceBusClient(_ServieBusConnection);
            ServiceBusSender sender = client.CreateSender(TopicName);
            try
            {
                await sender.SendMessagesAsync(DataEvents);
                DataEvents.Clear();
            }
            catch (Exception e)
            {
                var x = e.Message;
                try
                {
                    await sender.SendMessagesAsync(DataEvents);
                    DataEvents.Clear();
                }
                catch 
                { 
                    var x2 = e.Message;
                    DataEvents.Clear();
                }
                
            }
        }

        public async Task OnTransactionFailed()
        {
            DataEvents.Clear();
        }

    }


    public class ServiceBusDataEventPublisherOptions
    {
        public string TopicName { get; set; }
        private string ServieBusConnection { get; set; }
        public virtual bool PublishObjectUpdates { get; set; } = true;
        public virtual bool PublishObjectCreated { get; set; } = true;
        public virtual bool PublishObjectDeleted { get; set; } = true;
    }


}