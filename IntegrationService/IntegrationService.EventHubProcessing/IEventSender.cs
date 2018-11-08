using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Entities;

namespace IntegrationService.EventHubProcessing
{
    public interface IEventSender
    {
        bool CanSend(IEvent sendingEvent);
        void SendEvent(IEvent sendingEvent);
        void SendEvents(IEnumerable<IEvent> sendingEvents);
        Task SendEventAsync(IEvent sendingEvent);
    }
}
