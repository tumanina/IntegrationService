using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Consumers.Product;
using IntegrationService.EventHubProcessing.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IntegrationService.Business;
using IntegrationService.Business.DTO;
using Newtonsoft.Json;

namespace IntegrationService.Unit.Tests
{
    [TestClass]
    public class ConsumersTest
    {
        private static readonly Mock<IProductService> ProductService = new Mock<IProductService>();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
        }

        [TestMethod]
        public void ProductCreateConsumer_CreateProductMessage_ReturnsCorrect()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { ItemId = itemId, EventAction = "Create" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductCreateConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsTrue(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 1);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Once);
        }

        [TestMethod]
        public void ProductCreateConsumer_UpdateProductMessage_CannotConsume()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { ItemId = itemId, EventAction = "Change" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List <ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductCreateConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsFalse(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 0);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Never);
        }

        [TestMethod]
        public void ProductCreateConsumer_CreateProductNoItemId_CannotConsume()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { EventAction = "Create" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductCreateConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsFalse(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 0);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Never);
        }

        [TestMethod]
        public void ProductChangeConsumer_UpdateProductMessage_ReturnsCorrect()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { ItemId = itemId, EventAction = "Create" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductCreateConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsTrue(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 1);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Once);
        }

        [TestMethod]
        public void ProductChangeConsumer_CreateProductMessage_CannotConsume()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { ItemId = itemId, EventAction = "Create" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductChangeConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsFalse(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 0);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Never);
        }

        [TestMethod]
        public void ProductChangeConsumer_ChangeProductNoItemId_CannotConsume()
        {
            ProductService.ResetCalls();
            
            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { EventAction = "Change" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductChangeConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsFalse(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 0);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Never);
        }

        [TestMethod]
        public void ProductDeleteConsumer_DeleteProductMessage_GenerateProductIdAndCorrectEventAction()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { ItemId = itemId, EventAction = "Delete" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductDeleteConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsTrue(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 1);
            Assert.AreEqual(items.Result.FirstOrDefault().EventAction, EventAction.Deleted);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Once);
        }

        [TestMethod]
        public void ProductDeleteConsumer_CreateProductMessage_CannotConsume()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { ItemId = itemId, EventAction = "Create" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductDeleteConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsFalse(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 0);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Never);
        }

        [TestMethod]
        public void ProductDeleteConsumer_DeleteProductNoItemId_CannotConsume()
        {
            ProductService.ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";

            var message = new Message { EventAction = "Delete" };

            ProductService.Setup(x => x.GetProductInCountries(itemId, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey, CountryId = countryId } }));

            var consumer = new ProductDeleteConsumer(ProductService.Object);

            var items = consumer.Consume(message);

            Assert.IsFalse(consumer.CanConsume(message));
            Assert.AreEqual(items.Result.Count(), 0);
            ProductService.Verify(x => x.GetProductInCountries(itemId, null, null), Times.Never);
        }
    }
}
