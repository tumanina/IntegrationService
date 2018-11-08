using System;
using System.Collections.Generic;

namespace IntegrationService.EventHubProcessing.Entities
{
    public class Message
    {
        public Guid IntegrationMessageId { get; set; }
        public string EventAction { get; set; }
        public Guid SizeId { get; set; }
        public Guid ItemId { get; set; }
        public Guid? SaleId { get; set; }
        public string CountryId { get; set; }
        public List<Guid> AdminEventIds { get; set; }
    }
}
