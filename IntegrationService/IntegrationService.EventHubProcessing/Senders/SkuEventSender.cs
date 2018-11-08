using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApacKernel.AspNet;
using IntegrationService.EventHubProcessing.Entities;
using Microsoft.ServiceBus.Messaging;

namespace IntegrationService.EventHubProcessing.Senders
{
    public class SkuEventSender : ISkuEventSender
    {
        private readonly IEventHubSender _eventHubSender;
        private readonly IEnumerable<string> _countryIds;

        public SkuEventSender(IEventHubSender eventHubSender, IEnumerable<string> countryIds)
        {
            _eventHubSender = eventHubSender;
            _countryIds = countryIds;
        }

        public bool CanSend(IEvent skuEvent)
        {
            return !_countryIds.Any() || _countryIds.Any(t => t.ToLower() == skuEvent.CountryId.ToLower());
        }

        public void SendEvent(IEvent sendingEvent)
        {
            var data = new EventData(Encoding.UTF8.GetBytes(Json.ToJson(sendingEvent)));
            _eventHubSender.Send(data);
        }

        public void SendEvents(IEnumerable<IEvent> sendingEvents)
        {
            var events = sendingEvents.Select(sendingEvent => new EventData(Encoding.UTF8.GetBytes(Json.ToJson(sendingEvent)))).ToList();
            _eventHubSender.Send(events);
        }

        public async Task SendEventAsync(IEvent sendingEvent)
        {
            var data = new EventData(Encoding.UTF8.GetBytes(Json.ToJson(sendingEvent)));
            await _eventHubSender.SendAsync(data);
        }
    }
}
