using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Admin.Client;
using Admin.Client.Entities;

namespace IntegrationService.Business.Clients.Admin
{
    public class AdminApiClient : IAdminApiClient
    {
        private readonly IAdminRestClient _adminClient;

        public AdminApiClient(IAdminRestClient adminClient)
        {
            _adminClient = adminClient;
        }

        public Item GetItem(Guid itemId, Guid saleId, string countryId)
        { 
            return _adminClient.GetItem(itemId, saleId, countryId);
        }

        public Size GetSize(Guid itemId, Guid sizeId, Guid saleId, string countryId)
        {
            return _adminClient.GetSize(sizeId, itemId, saleId, countryId);
        }

        public IEnumerable<Size> GetItemSizes(Guid itemId, Guid saleId, string countryId)
        {
            return _adminClient.GetItemSizes(itemId, saleId, countryId);
        }

        public SaleInfo GetSale(Guid saleId)
        {
            return _adminClient.GetSaleInfo(saleId);
        }

        public IEnumerable<LocalizedSize> GetLoсalizedSizes(Guid itemId, Guid saleId, string countryId)
        {
            return _adminClient.GetLocalizedSizes(itemId, saleId, countryId);
        }

        public IEnumerable<SaleInCountry> GetSalesInCountriesByItemId(Guid itemId)
        {
            return _adminClient.GetSalesInCountriesByItemId(itemId);
        }

        public async Task<IEnumerable<SaleInCountry>> GetSalesInCountriesByItemIdAsync(Guid itemId)
        {
            return await _adminClient.GetSalesInCountriesByItemIdAsync(itemId);
        }

        public IEnumerable<string> GetCountriesBySaleId(Guid saleId)
        {
            return _adminClient.GetCountriesBySaleId(saleId);
        }

        public async Task<IEnumerable<string>> GetCountriesInWorkBySaleIdAsync(Guid saleId)
        {
            return await _adminClient.GetCountriesInWorkBySaleIdAsync(saleId);
        }

        public async Task<IEnumerable<string>> GetCountriesBySaleIdAsync(Guid saleId)
        {
            return await _adminClient.GetCountriesBySaleIdAsync(saleId);
        }

        public IEnumerable<ItemInSale> GetReconciliationData(string countryId, DateTime since, DateTime? till = null)
        {
            return _adminClient.GetReconciliationData(countryId, since, till);
        }

        public IEnumerable<SaleItem> GetActiveSaleItems(string countryId, DateTime date)
        {
            return _adminClient.GetActiveSaleItems(countryId, date);
        }

        public IEnumerable<Guid> GetActiveSales(string countryId, DateTime date)
        {
            return _adminClient.GetActiveSales(countryId, date);
        }

        public int GetItemsCount(string countryId, DateTime since, DateTime? till = null)
        {
            return _adminClient.GetItemsCount(countryId, since, till);
        }

        public LocalizedItem GetLoсalizedItem(Guid itemId, Guid saleId, string countryId)
        {
            return _adminClient.GetLocalizedItem(itemId:itemId, saleId: saleId, countryId: countryId);
        }

        public LocalizedSize GetLoсalizedSize(Guid itemId, Guid sizeId, Guid saleId, string countryId)
        {
            return _adminClient.GetLocalizedSize(sizeID: sizeId, itemId:itemId, saleId: saleId, countryId: countryId);
        }
    }
}