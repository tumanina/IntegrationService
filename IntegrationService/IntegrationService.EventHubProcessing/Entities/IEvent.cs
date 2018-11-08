using System;
using System.Collections.Generic;

namespace IntegrationService.EventHubProcessing.Entities
{
    public interface IEvent
    {
        EventAction EventAction { get; }

        EventType EventType { get; }

        string Id { get; }

        string CountryId { get; }

        IEnumerable<string> CountryIds { get; }

        IEnumerable<Guid> AdminEventIds { get; }

        Guid EventId { get; }
    }
}
