using System;
using System.Collections.Generic;

namespace IntegrationService.EventHubProcessing.Entities.Events
{
    public class BaseInnerEvent : IEvent
    {
        public BaseInnerEvent(EventType eventType)
        {
            EventType = eventType;
            CountryIds = new List<string>();
        }

        public EventType EventType { get; }
        public EventAction EventAction { get; set; }
        public string Id { get; set; }
        public string CountryId { get; set; }
        public IEnumerable<string> CountryIds { get; set; }
        public IEnumerable<Guid> AdminEventIds { get; set; }
        public Guid EventId { get; set; }
    }
}
