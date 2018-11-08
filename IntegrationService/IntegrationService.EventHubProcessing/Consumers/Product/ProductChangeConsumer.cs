using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Entities;
using IntegrationService.EventHubProcessing.Entities.Events;
using IntegrationService.Business;

namespace IntegrationService.EventHubProcessing.Consumers.Product
{
    public class ProductChangeConsumer : IProductEventConsumer
    {
        private readonly IProductService _productService;

        public ProductChangeConsumer(IProductService productService)
        {
            _productService = productService;
        }

        public bool CanConsume(Message message)
        {
            return message != null
                   && (message.ItemId != Guid.Empty)
                   && message.EventAction == "Change";
        }

        public async Task<IEnumerable<IEvent>> Consume(Message message)
        {
            if (CanConsume(message))
            {
                var products = await _productService.GetProductInCountries(message.ItemId, message.SaleId, message.CountryId);

                return products.Select(product => new ProductEvent
                {
                    EventId = Guid.NewGuid(),
                    Id = product.Id,
                    EventAction = EventAction.Updated,
                    CountryId = product.CountryId,
                    AdminEventIds = message.AdminEventIds
                });
            }

            return new List<ProductEvent>();
        }
    }
}
