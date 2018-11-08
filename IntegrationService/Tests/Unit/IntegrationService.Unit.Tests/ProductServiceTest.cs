using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ApacKernel.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Admin.Client.Entities;
using IntegrationService.Business;
using IntegrationService.Business.Clients.Admin;

namespace IntegrationService.Unit.Tests
{
    [TestClass]
    public class ProductServiceTest
    {
        private static readonly Mock<IAdminApiClient> AdminClient = new Mock<IAdminApiClient>();
        private static readonly Mock<ITaxonomyApiClient> TaxonomyClient = new Mock<ITaxonomyApiClient>();

        [TestMethod]
        public void GetProduct_ReturnProductCorrectIdCorrectAttributesUseClient()
        {
            AdminClient.ResetCalls();

            var itemId = Guid.NewGuid();
            var saleId = Guid.NewGuid();
            var countryId = "AS";

            var item = new Item
            {
                ID = itemId,
                Name = "test item",
                TaxonomyDetails = new List<TaxonomyDetail>(),
                Sku = "sku",
                Sizes = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },
                Price = new ItemPrice { OzsalePrice = new PriceInfo { Currency = "AUD", Value = 10 }, RegularPrice = new PriceInfo { Currency = "AUD", Value = 20 } }
            };

            var productId = $"{itemId}_{saleId}_{countryId}";

            AdminClient.Setup(x => x.GetItem(itemId, saleId, countryId)).Returns(item);

            var service = new ProductService(AdminClient.Object, TaxonomyClient.Object, new Dictionary<string, Guid>());

            var product = service.GetProduct(itemId, saleId, countryId);

            AdminClient.Verify(x => x.GetItem(itemId, saleId, countryId), Times.Once);
            Assert.AreEqual(product.Id, productId);
            Assert.IsTrue(product.Attributes.ContainsKey("SaleId"));
            Assert.IsTrue(product.Attributes.ContainsKey("SiteId"));
            Assert.AreEqual(product.Attributes["SaleId"], saleId.ToHash());
            Assert.AreEqual(product.Attributes["SiteId"], countryId);
            Assert.AreEqual(product.SkuIds.Length, 2);
        }

        [TestMethod]
        public void GetProduct_ProductByIdNotFound_ReturnNullNoException()
        {
            AdminClient.ResetCalls();

            var itemId = Guid.NewGuid();
            var saleId = Guid.NewGuid();
            var countryId = "AS";

            AdminClient.Setup(x => x.GetItem(itemId, saleId, countryId)).Returns((Item)null);

            var service = new ProductService(AdminClient.Object, TaxonomyClient.Object, new Dictionary<string, Guid>());

            try
            {
                var product = service.GetProduct(itemId, saleId, countryId);

                AdminClient.Verify(x => x.GetItem(itemId, saleId, countryId), Times.Once);
                Assert.AreEqual(product, null);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetProduct_CountryInTaxonomySettings_UseTaxonomyClientReturnCategoriesFromTaxonomy()
        {
            AdminClient.ResetCalls();

            var itemId = Guid.NewGuid();
            var saleId = Guid.NewGuid();
            var countryId = "AS";
            var taxonomyId = Guid.NewGuid();
            var treeId = Guid.NewGuid();

            var item = new Item
            {
                ID = itemId,
                Name = "test item",
                TaxonomyId = taxonomyId,
                TaxonomyDetails = new List<TaxonomyDetail> { new TaxonomyDetail { Key = 0, Value = "womens"} },
                Sku = "sku",
                Sizes = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },
                Price = new ItemPrice { OzsalePrice = new PriceInfo { Currency = "AUD", Value = 10 }, RegularPrice = new PriceInfo { Currency = "AUD", Value = 20 } }
            };

            var productId = $"{itemId}_{saleId}_{countryId}";

            AdminClient.Setup(x => x.GetItem(itemId, saleId, countryId)).Returns(item);
            TaxonomyClient.Setup(x => x.GetTaxonomyTree(taxonomyId, treeId)).Returns(new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "mens") });

            var service = new ProductService(AdminClient.Object, TaxonomyClient.Object, new Dictionary<string, Guid> { { countryId, treeId } });

            var product = service.GetProduct(itemId, saleId, countryId);

            AdminClient.Verify(x => x.GetItem(itemId, saleId, countryId), Times.Once);
            TaxonomyClient.Verify(x => x.GetTaxonomyTree(taxonomyId, treeId), Times.Once);
            Assert.AreEqual(product.Id, productId);
            Assert.AreEqual(product.TaxonomyTree.Count, 1);
            Assert.AreEqual(product.TaxonomyTree.FirstOrDefault().Value, "mens");
            Assert.IsTrue(product.Attributes.ContainsKey("SaleId"));
            Assert.IsTrue(product.Attributes.ContainsKey("SiteId"));
            Assert.AreEqual(product.Attributes["SaleId"], saleId.ToHash());
            Assert.AreEqual(product.Attributes["SiteId"], countryId);
            Assert.AreEqual(product.SkuIds.Length, 2);
        }

        [TestMethod]
        public void GetProduct_CountryNotInTaxonomySettings_NotUseTaxonomyClientReturnCategoriesFromAdmin()
        {
            AdminClient.ResetCalls();

            var itemId = Guid.NewGuid();
            var saleId = Guid.NewGuid();
            var countryId = "AS";
            var taxonomyId = Guid.NewGuid();
            var treeId = Guid.NewGuid();

            var item = new Item
            {
                ID = itemId,
                Name = "test item",
                TaxonomyId = taxonomyId,
                TaxonomyDetails = new List<TaxonomyDetail> { new TaxonomyDetail { Key = 0, Value = "womens" } },
                Sku = "sku",
                Sizes = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },
                Price = new ItemPrice { OzsalePrice = new PriceInfo { Currency = "AUD", Value = 10 }, RegularPrice = new PriceInfo { Currency = "AUD", Value = 20 } }
            };

            var productId = $"{itemId}_{saleId}_{countryId}";

            AdminClient.Setup(x => x.GetItem(itemId, saleId, countryId)).Returns(item);
            TaxonomyClient.Setup(x => x.GetTaxonomyTree(taxonomyId, treeId)).Returns(new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "mens") });

            var service = new ProductService(AdminClient.Object, TaxonomyClient.Object, new Dictionary<string, Guid> { { "DA", treeId } });

            var product = service.GetProduct(itemId, saleId, countryId);

            AdminClient.Verify(x => x.GetItem(itemId, saleId, countryId), Times.Once);
            TaxonomyClient.Verify(x => x.GetTaxonomyTree(taxonomyId, treeId), Times.Never);
            Assert.AreEqual(product.Id, productId);
            Assert.AreEqual(product.TaxonomyTree.Count, 1);
            Assert.AreEqual(product.TaxonomyTree.FirstOrDefault().Value, "womens");
            Assert.IsTrue(product.Attributes.ContainsKey("SaleId"));
            Assert.IsTrue(product.Attributes.ContainsKey("SiteId"));
            Assert.AreEqual(product.Attributes["SaleId"], saleId.ToHash());
            Assert.AreEqual(product.Attributes["SiteId"], countryId);
            Assert.AreEqual(product.SkuIds.Length, 2);
        }

        [TestMethod]
        public void GetProduct_CountryInTaxonomySettingsTaxonomyClientReturnsNull_UseTaxonomyClientReturnNull()
        {
            AdminClient.ResetCalls();

            var itemId = Guid.NewGuid();
            var saleId = Guid.NewGuid();
            var countryId = "AS";
            var taxonomyId = Guid.NewGuid();
            var treeId = Guid.NewGuid();

            var item = new Item
            {
                ID = itemId,
                Name = "test item",
                TaxonomyId = taxonomyId,
                TaxonomyDetails = new List<TaxonomyDetail> { new TaxonomyDetail { Key = 0, Value = "womens" } },
                Sku = "sku",
                Sizes = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },
                Price = new ItemPrice { OzsalePrice = new PriceInfo { Currency = "AUD", Value = 10 }, RegularPrice = new PriceInfo { Currency = "AUD", Value = 20 } }
            };

            AdminClient.Setup(x => x.GetItem(itemId, saleId, countryId)).Returns(item);
            TaxonomyClient.Setup(x => x.GetTaxonomyTree(taxonomyId, treeId)).Returns((List<KeyValuePair<int, string>>)null);

            var service = new ProductService(AdminClient.Object, TaxonomyClient.Object, new Dictionary<string, Guid> { { countryId, treeId } });

            var product = service.GetProduct(itemId, saleId, countryId);

            AdminClient.Verify(x => x.GetItem(itemId, saleId, countryId), Times.Once);
            TaxonomyClient.Verify(x => x.GetTaxonomyTree(taxonomyId, treeId), Times.Once);
            Assert.AreEqual(product, null);
        }

        [TestMethod]
        public void GetProduct_CountryInTaxonomySettingsTaxonomyIdIsEmpty_DoNotUseTaxonomyClientReturnNull()
        {
            AdminClient.ResetCalls();

            var itemId = Guid.NewGuid();
            var saleId = Guid.NewGuid();
            var countryId = "AS";
            var taxonomyId = Guid.NewGuid();
            var treeId = Guid.NewGuid();

            var item = new Item
            {
                ID = itemId,
                Name = "test item",
                TaxonomyId = Guid.Empty,
                TaxonomyDetails = new List<TaxonomyDetail>(),
                Sku = "sku",
                Sizes = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },
                Price = new ItemPrice { OzsalePrice = new PriceInfo { Currency = "AUD", Value = 10 }, RegularPrice = new PriceInfo { Currency = "AUD", Value = 20 } }
            };

            AdminClient.Setup(x => x.GetItem(itemId, saleId, countryId)).Returns(item);
            TaxonomyClient.Setup(x => x.GetTaxonomyTree(taxonomyId, treeId)).Returns((List<KeyValuePair<int, string>>)null);

            var service = new ProductService(AdminClient.Object, TaxonomyClient.Object, new Dictionary<string, Guid> { { countryId, treeId } });

            var product = service.GetProduct(itemId, saleId, countryId);

            AdminClient.Verify(x => x.GetItem(itemId, saleId, countryId), Times.Once);
            TaxonomyClient.Verify(x => x.GetTaxonomyTree(taxonomyId, treeId), Times.Never);
            Assert.AreEqual(product, null);
        }
    }
}
