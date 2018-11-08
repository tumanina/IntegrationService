using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Admin.Client.Entities;

namespace IntegrationService.Business.Clients.Admin
{
    public interface IAdminApiClient
    {
        Item GetItem(Guid itemId, Guid saleId, string countryId);
        Size GetSize(Guid itemId, Guid sizeId, Guid saleId, string countryId);
        IEnumerable<Size> GetItemSizes(Guid itemId, Guid saleId, string countryId);
        SaleInfo GetSale(Guid saleId);
        LocalizedItem GetLoсalizedItem(Guid itemId, Guid saleId, string countryId);
        LocalizedSize GetLoсalizedSize(Guid itemId, Guid sizeId, Guid saleId, string countryId);
        IEnumerable<LocalizedSize> GetLoсalizedSizes(Guid itemId, Guid saleId, string countryId);
        IEnumerable<SaleInCountry> GetSalesInCountriesByItemId(Guid itemId);
        Task<IEnumerable<SaleInCountry>> GetSalesInCountriesByItemIdAsync(Guid itemId);
        IEnumerable<string> GetCountriesBySaleId(Guid saleId);
        Task<IEnumerable<string>> GetCountriesInWorkBySaleIdAsync(Guid saleId);
        Task<IEnumerable<string>> GetCountriesBySaleIdAsync(Guid saleId);
        IEnumerable<ItemInSale> GetReconciliationData(string countryId, DateTime since, DateTime? till = null);
        IEnumerable<SaleItem> GetActiveSaleItems(string countryId, DateTime date);
        IEnumerable<Guid> GetActiveSales(string countryId, DateTime date);
        int GetItemsCount(string countryId, DateTime since, DateTime? till = null);
    }
}