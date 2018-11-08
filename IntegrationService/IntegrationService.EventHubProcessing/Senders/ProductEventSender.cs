using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApacKernel.AspNet;
using IntegrationService.EventHubProcessing.Entities;
using Microsoft.ServiceBus.Messaging;

namespace IntegrationService.EventHubProcessing.Senders
{
    public class ProductEventSender : IProductEventSender
    {
        private readonly IEventHubSender _eventHubSender;
        private readonly IEnumerable<string> _countryIds;

        public ProductEventSender(IEventHubSender eventHubSender, IEnumerable<string> countryIds)
        {
            _eventHubSender = eventHubSender;
            _countryIds = countryIds;
        }

        public bool CanSend(IEvent productEvent)
        {
            return !_countryIds.Any() || _countryIds.Any(t => t.ToLower() == productEvent.CountryId.ToLower());
        }

        public bool CanRefeed(string countryId)
        {
            return _countryIds.Any(t => t.ToLower() == countryId.ToLower());
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

        public void SendRefeedEvent(IEvent sendingEvent)
        {
            var data = new EventData(Encoding.UTF8.GetBytes(Json.ToJson(sendingEvent)));
            data.Properties.Add("EventType", "Refeed");
            _eventHubSender.Send(data);
        }

        public void SendRefeedEvents(IEnumerable<IEvent> sendingEvents)
        {
            var events = new List<EventData>();

            foreach (var sendingEvent in sendingEvents)
            {
                var data = new EventData(Encoding.UTF8.GetBytes(Json.ToJson(sendingEvent)));
                data.Properties.Add("EventType", "Refeed");
                events.Add(data);
            }

            _eventHubSender.Send(events);
        }
    }
}