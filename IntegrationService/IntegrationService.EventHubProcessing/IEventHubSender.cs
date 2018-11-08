using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IntegrationService.EventHubProcessing
{
    public interface IEventHubSender
    {
        void Send(EventData eventData);
        void Send(IEnumerable<EventData> events);
        Task SendAsync(EventData eventData);
    }
}
