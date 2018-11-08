using System.Collections.Generic;
using IntegrationService.EventHubProcessing.Entities;

namespace IntegrationService.EventHubProcessing.Senders
{
    public interface IProductEventSender : IEventSender
    {
        bool CanRefeed(string countryId);
        void SendRefeedEvent(IEvent sendingEvent);
        void SendRefeedEvents(IEnumerable<IEvent> sendingEvents);
    }
}
