using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Entities;
using IntegrationService.EventHubProcessing.Entities.Events;
using IntegrationService.Business;

namespace IntegrationService.EventHubProcessing.Consumers.Sku
{
    public class SkuDeleteConsumer : ISkuEventConsumer
    {
        private readonly IProductService _productService;

        public SkuDeleteConsumer(IProductService productService)
        {
            _productService = productService;
        }

        public bool CanConsume(Message message)
        {
            return message != null
                   && (message.ItemId != Guid.Empty)
                   && message.EventAction == "Delete";
        }

        public async Task<IEnumerable<IEvent>> Consume(Message message)
        {
            if (CanConsume(message))
            {
                var products = await _productService.GetProductInCountries(message.ItemId, message.SaleId, message.CountryId);

                return products.Select(product => new SkuEvent
                {
                    Id = $"{message.SizeId}_{product.Id}",
                    EventAction = EventAction.Deleted,
                    EventId = Guid.NewGuid(),
                    CountryId = product.CountryId,
                    AdminEventIds = message.AdminEventIds
                });
            }

            return new List<SkuEvent>();
        }
    }
}
