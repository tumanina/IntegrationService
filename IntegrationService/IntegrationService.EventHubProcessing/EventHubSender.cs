using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IntegrationService.EventHubProcessing
{
    public class EventHubSender : IEventHubSender
    {
        private readonly EventHubClient _eventHubClient;

        public EventHubSender(string connectionString)
        {
            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);
        }

        public void Send(EventData eventData)
        {
            _eventHubClient.Send(eventData);
        }

        public void Send(IEnumerable<EventData> events)
        {
            _eventHubClient.SendBatch(events);
        }

        public async Task SendAsync(EventData eventData)
        {
            await _eventHubClient.SendAsync(eventData);
        }
    }
}