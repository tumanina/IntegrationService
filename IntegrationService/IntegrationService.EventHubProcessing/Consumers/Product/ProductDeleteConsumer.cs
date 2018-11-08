using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Entities;
using IntegrationService.EventHubProcessing.Entities.Events;
using IntegrationService.Business;

namespace IntegrationService.EventHubProcessing.Consumers.Product
{
    public class ProductDeleteConsumer : IProductEventConsumer
    {
        private readonly IProductService _productService;

        public ProductDeleteConsumer(IProductService productService)
        {
            _productService = productService;
       }

        public bool CanConsume(Message model)
        {
            return model != null
                   && (model.ItemId != Guid.Empty)
                   && model.EventAction == "Delete";
        }

        public async Task<IEnumerable<IEvent>> Consume(Message message)
        {
            if (CanConsume(message))
            {
                var products = await _productService.GetProductInCountries(message.ItemId, message.SaleId, message.CountryId);
                return products.Select(product => new ProductEvent
                {
                    Id = product.Id,
                    EventId = Guid.NewGuid(),
                    EventAction = EventAction.Deleted,
                    CountryId = product.CountryId,
                    AdminEventIds = message.AdminEventIds
                });
            }

            return new List<ProductEvent>();
        }
    }
}
