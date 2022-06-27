using Azure.Messaging.ServiceBus;
using EntityFramework.DataEvent.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace EntityFramework.DataEvent.AzureServiceBus
{


    public class TopicPerTypeServiceBusDataEventPublisher : IDataEventPublisher
    {
        public Object RequestContext { get; set; }
        public List<TptServiceBusMessage> DataEvents { get; set; } = new List<TptServiceBusMessage>();
        private string _ServieBusConnection { get; set; }
        public virtual bool PublishObjectUpdates { get; set; } = true;
        public virtual bool PublishObjectCreated { get; set; } = true;
        public virtual bool PublishObjectDeleted { get; set; } = true;



        public TopicPerTypeServiceBusDataEventPublisher(IOptions<TPTServiceBusDataEventPublisherOptions> options, IConfiguration configuration)
        {
            var ops = options.Value;
            this.PublishObjectCreated = ops.PublishObjectCreated;
            this.PublishObjectDeleted = ops.PublishObjectDeleted;
            this.PublishObjectUpdates = ops.PublishObjectUpdates;
            if(!string.IsNullOrEmpty(ops.ServieBusConnection))
            {
                _ServieBusConnection = ops.ServieBusConnection;
            }
            else
            _ServieBusConnection = configuration?.GetValue<string>("ServiceBusDataEventPublisherConnection");
        }


        public async Task OnDataItemCreated(object data, string correlationId)
        {
            if (PublishObjectCreated)
            {
                IDataEventEntity? dataEvent = data as IDataEventEntity;
                if (dataEvent != null)
                {
                    var msgBody = JsonSerializer.Serialize(data);

                    TptServiceBusMessage serviceBusMessage = new TptServiceBusMessage(Encoding.UTF8.GetBytes(msgBody));
                    serviceBusMessage.CorrelationId = correlationId;
                    serviceBusMessage.ApplicationProperties.Add("requestContext", RequestContext);
                    serviceBusMessage.ContentType = "/" + dataEvent.GetDataType();
                    serviceBusMessage.Subject = "created";
                    serviceBusMessage.Topic = dataEvent.GetDataType().ToString();
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
                    TptServiceBusMessage serviceBusMessage = new TptServiceBusMessage(Encoding.UTF8.GetBytes(msgBody));
                    serviceBusMessage.ContentType = "/" + dataEvent.GetDataType();
                    serviceBusMessage.Subject = "deleted";
                    serviceBusMessage.CorrelationId = correlationId;
                    serviceBusMessage.ApplicationProperties.Add("requestContext", RequestContext);
                    serviceBusMessage.Topic = dataEvent.GetDataType().ToString();
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
                    TptServiceBusMessage serviceBusMessage = new TptServiceBusMessage(Encoding.UTF8.GetBytes(msgBody));
                    serviceBusMessage.ContentType = "/" + dataEvent.GetDataType();
                    serviceBusMessage.Subject = "updated";
                    serviceBusMessage.CorrelationId = correlationId;
                    serviceBusMessage.ApplicationProperties.Add("requestContext", RequestContext);
                    serviceBusMessage.Topic = dataEvent.GetDataType().ToString();
                    DataEvents.Add(serviceBusMessage);
                }
            }
        }

        public async Task OnTransactionCompleted()
        {
            var dictionary = DataEvents.GroupBy(e => e.Topic);
            await using var client = new ServiceBusClient(_ServieBusConnection);
            foreach(var item in dictionary)
            {
                var values = item.ToList();
                ServiceBusSender sender = client.CreateSender(item.Key);
                try
                {
                    await sender.SendMessagesAsync(values);
                    
                }
                catch (Exception e)
                {
                    var x = e.Message;
                    try
                    {
                        await sender.SendMessagesAsync(values);
                    }
                    catch
                    {
                        //todo write to log
                        var x2 = e.Message;
                    }

                }
            }
            DataEvents.Clear();


        }

        public async Task OnTransactionFailed()
        {
            DataEvents.Clear();
        }

    }


    public class TPTServiceBusDataEventPublisherOptions
    {
        public string ServieBusConnection { get; set; }
        public virtual bool PublishObjectUpdates { get; set; } = true;
        public virtual bool PublishObjectCreated { get; set; } = true;
        public virtual bool PublishObjectDeleted { get; set; } = true;
    }

    public class TptServiceBusMessage : ServiceBusMessage
    {
        public string Topic { get; set; }
        public TptServiceBusMessage() : base()
        { }
        public TptServiceBusMessage(string body):base(body)
        {

        }
        public TptServiceBusMessage(BinaryData body): base(body)
        { }
        public TptServiceBusMessage (ReadOnlyMemory<byte> body) : base(body)
        { }
    }
}
