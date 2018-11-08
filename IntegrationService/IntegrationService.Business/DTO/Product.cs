using System;
using System.Collections.Generic;

namespace IntegrationService.Business.DTO
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PriceDescription { get; set; }
        public string OriginalPriceDescription { get; set; }
        public Price Price { get; set; }
        public Price OriginalPrice { get; set; }
        public string[] Images { get; set; }
        public string[] SkuIds { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public Dictionary<int, string> TaxonomyTree { get; set; }
        public bool IsAvailable { get; set; }
        public int QuantitySold { get; set; }
        public Guid MasterProductId { get; set; }
    }
}
