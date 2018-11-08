using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Entities;
using IntegrationService.EventHubProcessing.Entities.Events;
using IntegrationService.Business;

namespace IntegrationService.EventHubProcessing.Consumers.Product
{
    public class ProductCreateConsumer : IProductEventConsumer
    {
        private readonly IProductService _productService;

        public ProductCreateConsumer(IProductService productService)
        {
            _productService = productService;
        }

        public bool CanConsume(Message message)
        {
            return message != null
                   && (message.ItemId != Guid.Empty)
                   && message.EventAction == "Create";
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
                    EventAction = EventAction.Added,
                    CountryId = product.CountryId,
                    AdminEventIds = message.AdminEventIds
                });
            }

            return new List<ProductEvent>();
        }
    }
}
