using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApacKernel.Diagnostics;
using ApacKernel.Extensions;
using IntegrationService.Business.Clients.Admin;
using IntegrationService.Business.DTO;
using Product = IntegrationService.Business.DTO.Product;

namespace IntegrationService.Business
{
    public class ProductService : IProductService
    {
        private readonly IAdminApiClient _adminClient;
        private readonly ITaxonomyApiClient _taxonomyApiClient;
        private readonly Dictionary<string, Guid> _taxonomyTrees;
        protected Log Log = Log.GetLogger<ProductService>();

        public ProductService(IAdminApiClient adminClient, ITaxonomyApiClient taxonomyApiClient, Dictionary<string, Guid> taxonomyTrees)
        {
            _adminClient = adminClient;
            _taxonomyApiClient = taxonomyApiClient;
            _taxonomyTrees = taxonomyTrees;
        }

        public Product GetProduct(Guid itemId, Guid saleId, string countryId)
        {
            try
            {
                var item = _adminClient.GetItem(itemId, saleId, countryId);

                if (item == null || item.ID == Guid.Empty)
                {
                    return null;
                }

                var taxonomies = item.TaxonomyDetails.ToDictionary(taxonomyDetail => taxonomyDetail.Key,
                    taxonomyDetail => taxonomyDetail.Value);

                foreach (var taxonomyTree in _taxonomyTrees)
                {
                    if (string.Equals(taxonomyTree.Key, countryId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (item.TaxonomyId == Guid.Empty)
                        {
                            Log.Error($"Taxonomy for item '{itemId}' is null or empty. ");
                            return null;
                        }

                        var countryTaxonomyTree = _taxonomyApiClient.GetTaxonomyTree(item.TaxonomyId, taxonomyTree.Value);

                        if (countryTaxonomyTree != null && countryTaxonomyTree.Any())
                        {
                            taxonomies = countryTaxonomyTree.ToDictionary(taxonomyDetail => taxonomyDetail.Key,
                                taxonomyDetail => taxonomyDetail.Value);
                        }
                        else
                        {
                            var taxonomyPath = taxonomies.Aggregate(string.Empty, (current, taxonomy) => current + (taxonomy.Value + ">"));
                            Log.Error($"Mapped taxonomy category for item '{itemId}' with main taxonomy '{taxonomyPath.TrimEnd('>')}' in DD tree not founded. ");
                            return null;
                        }
                    }
                }

                return new Product
                {
                    Name = item.Name,
                    Description = item.Description,
                    Id = $"{itemId}_{saleId}_{countryId}",
                    Attributes = new Dictionary<string, object>
                    {
                        { "ProductId", itemId.ToHash() },
                        { "SaleId", saleId.ToHash() },
                        { "SiteId", countryId }
                    },
                    TaxonomyTree = taxonomies,
                    SkuIds = GetSkuList(item.Sizes, itemId, saleId, countryId).ToArray(),
                    PriceDescription = ($"{item.Price.OzsalePrice.Value} {item.Price.OzsalePrice.Currency}"),
                    OriginalPriceDescription = ($"{item.Price.RegularPrice.Value} {item.Price.RegularPrice.Currency}"),
                    Price = new Price { Value = item.Price.OzsalePrice.Value , Currency = item.Price.OzsalePrice.Currency },
                    OriginalPrice = new Price { Value = item.Price.RegularPrice.Value, Currency = item.Price.RegularPrice.Currency },
                    IsAvailable = item.IsAvailable,
                    QuantitySold = item.QuantitySold,
                    Images = item.Images,
                    MasterProductId = itemId
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        public async Task<IEnumerable<ProductInCountry>> GetProductInCountries(Guid itemId, Guid? saleId, string countryId)
        {
            try
            {
                var products = new List<ProductInCountry>();
                if (!saleId.HasValue || string.IsNullOrEmpty(countryId))
                {
                    if (!saleId.HasValue)
                    {
                        var saleInCountries = await _adminClient.GetSalesInCountriesByItemIdAsync(itemId);
                        foreach (var saleInCountry in saleInCountries)
                        {
                            products.Add(new ProductInCountry { Id = GetProductId(itemId, saleInCountry.SaleID, saleInCountry.CountryID), CountryId = saleInCountry.CountryID });
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(countryId))
                        {
                            var countries = await _adminClient.GetCountriesBySaleIdAsync(saleId.Value);
                            foreach (var country in countries)
                            {
                                products.Add(new ProductInCountry { Id = GetProductId(itemId, saleId.Value, country), CountryId = country });
                            }
                        }
                    }
                }
                else
                {
                    products.Add(new ProductInCountry { Id = GetProductId(itemId, saleId.Value, countryId), CountryId = countryId });
                }

                return products;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        public IEnumerable<string> GetActiveProducts(string countryId, DateTime date)
        {
            var items = _adminClient.GetActiveSaleItems(countryId, date);

            return items.Select(item => GetProductId(item.ItemId, item.SaleId, countryId)).ToList();
        }


        private IEnumerable<string> GetSkuList(IEnumerable<Guid> sizes, Guid itemId, Guid saleId, string countryId)
        {
            return !sizes.Any() ? new List<string> { GetSkuId(string.Empty, itemId, saleId, countryId) } 
                    : sizes.Select(t => GetSkuId(t.ToString(), itemId, saleId, countryId));
        }

        private string GetSkuId(string sizeId, Guid itemId, Guid saleId, string countryId)
        {
            return $"{sizeId}_{itemId}_{saleId}_{countryId}";
        }

        private string GetProductId(Guid itemId, Guid saleId, string countryId)
        {
            return $"{itemId}_{saleId}_{countryId}";
        }
    }
}