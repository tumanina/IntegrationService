using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationService.Business.DTO;

namespace IntegrationService.Business
{
    public interface IProductService
    {
        Product GetProduct(Guid itemId, Guid saleId, string countryId);
        Task<IEnumerable<ProductInCountry>> GetProductInCountries(Guid itemId, Guid? saleId, string countryId);
        IEnumerable<string> GetActiveProducts(string countryId, DateTime date);
    }
}