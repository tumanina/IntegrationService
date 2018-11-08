using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing;
using IntegrationService.EventHubProcessing.Consumers.Product;
using IntegrationService.EventHubProcessing.Entities;
using IntegrationService.EventHubProcessing.Entities.Events;
using IntegrationService.EventHubProcessing.Senders;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IntegrationService.Business;
using IntegrationService.Business.DTO;
using Newtonsoft.Json;

namespace IntegrationService.Unit.Tests
{
    [TestClass]
    public class MessageProcessorTest
    {
        private static readonly Mock<IProductService> ProductService = new Mock<IProductService>();
        private static readonly Mock<IProductEventSender> ProductEventSender1 = new Mock<IProductEventSender>();
        private static readonly Mock<IProductEventSender> ProductEventSender2 = new Mock<IProductEventSender>();
        private static readonly Mock<IProductEventConsumer> ProductEventConsumer = new Mock<IProductEventConsumer>();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
        }

        [TestMethod]
        public void MessagesProcessor_NoEventTypeProperty_Ignore()
        {
            ResetCalls();

            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var encoding = new UTF8Encoding();

            var message = new Message { ItemId = itemId, EventAction = "Create" };
            var eventData = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message).ToLower()));
            var productEvent = new ProductEvent { CountryId = countryId };

            ProductEventConsumer.Setup(x => x.Consume(message)).Returns(Task.FromResult<IEnumerable<IEvent>>(new List<ProductEvent> { productEvent }));
            ProductEventSender1.Setup(x => x.SendEvent(It.IsAny<IEvent>()));
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()));

            var senders = new List<IProductEventSender> { ProductEventSender1.Object };
            var consumers = new List<ProductCreateConsumer> {new ProductCreateConsumer(ProductService.Object)};

            var service = new MessagesProcessor<ProductCreateConsumer, IProductEventSender>(consumers, senders);

            var items = service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData });

            ProductEventConsumer.Verify(x => x.Consume(message), Times.Never);
            ProductEventSender1.Verify(x => x.SendEvent(It.IsAny<IEvent>()), Times.Never);
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Never);
        }

        [TestMethod]
        public void MessagesProcessor_TwoSendersBothCanSend_ProcessedAllToBothSenders()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var saleId2 = new Guid("4832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId2 = new Guid("50E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var productKey2 = $"{itemId2}_{saleId2}_{countryId}";
            var encoding = new UTF8Encoding();

            var message1 = new Message { ItemId = itemId1, EventAction = "Create" };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId2, EventAction = "Create", IntegrationMessageId = Guid.NewGuid() };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();
            var sender2Events = new List<IEvent>(); 

            ProductService.Setup(x => x.GetProductInCountries(itemId1, null, null))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductService.Setup(x => x.GetProductInCountries(itemId2, null, null))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey2, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);
            ProductEventSender2.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { sender2Events.AddRange(productEvents); });
            ProductEventSender2.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object, ProductEventSender2.Object };
            var consumers = new List<ProductCreateConsumer> { new ProductCreateConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductCreateConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 2);
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey2));
            Assert.AreEqual(sender2Events.Count, 2);
            Assert.IsTrue(sender2Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender2Events.Any(t => t.Id == productKey2));
            Assert.IsTrue(sender2Events.FirstOrDefault().EventType == EventType.Product);
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Exactly(2));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Once);
            ProductEventSender2.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        [TestMethod]
        public void MessagesProcessor_TwoSendersOnlyOneCanSend_ProcessedAllToBothSenders()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var saleId2 = new Guid("4832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId2 = new Guid("50E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var productKey2 = $"{itemId2}_{saleId2}_{countryId}";
            var encoding = new UTF8Encoding();

            var message1 = new Message { ItemId = itemId1, EventAction = "Create", IntegrationMessageId = Guid.NewGuid() };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId2, EventAction = "Create", IntegrationMessageId = Guid.NewGuid() };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();
            var sender2Events = new List<IEvent>();

            ProductService.Setup(x => x.GetProductInCountries(itemId1, null, null))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductService.Setup(x => x.GetProductInCountries(itemId2, null, null))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey2, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(false);
            ProductEventSender2.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { sender2Events.AddRange(productEvents); });
            ProductEventSender2.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object, ProductEventSender2.Object };
            var consumers = new List<ProductCreateConsumer> { new ProductCreateConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductCreateConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 0);
            Assert.AreEqual(sender2Events.Count, 2);
            Assert.IsTrue(sender2Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender2Events.Any(t => t.Id == productKey2));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Exactly(2));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Never);
            ProductEventSender1.Verify(x => x.SendEvent(It.IsAny<IEvent>()), Times.Never);
            ProductEventSender2.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Once);
            ProductEventSender2.Verify(x => x.SendEvent(It.IsAny<IEvent>()), Times.Never);
        }

        [TestMethod]
        public void MessagesProcessor_ProductsInThreeCountriesSenderOnlyTwoCountries_ProcessedTwoMessageSendOneBatch()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId1 = "AS";
            var countryId2 = "TA";
            var countryId3 = "OO";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId1}";
            var productKey2 = $"{itemId1}_{saleId1}_{countryId2}";
            var productKey3 = $"{itemId1}_{saleId1}_{countryId3}";
            var encoding = new UTF8Encoding();

            var message1 = new Message { ItemId = itemId1, EventAction = "Create", IntegrationMessageId = Guid.NewGuid() };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);
            
            var senderEvents = new List<IEvent>();

            ProductService.Setup(x => x.GetProductInCountries(itemId1, null, null))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry>
                {
                    new ProductInCountry { Id = productKey1, CountryId = countryId1 },
                    new ProductInCountry { Id = productKey2, CountryId = countryId2 },
                    new ProductInCountry { Id = productKey3, CountryId = countryId3 }
                }));


            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { senderEvents.AddRange(productEvents); });
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns<IEvent>((IEvent evt) => (evt.CountryId == countryId1 || evt.CountryId == countryId3) ? true : false);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object, ProductEventSender2.Object };
            var consumers = new List<ProductCreateConsumer> { new ProductCreateConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductCreateConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1 }).ConfigureAwait(true);

            Assert.AreEqual(senderEvents.Count, 2);
            Assert.IsTrue(senderEvents.Any(t => t.Id == productKey1));
            Assert.IsTrue(senderEvents.Any(t => t.Id == productKey3));
            Assert.IsTrue(senderEvents.FirstOrDefault().EventType == EventType.Product);
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Once);
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Once);
            ProductEventSender1.Verify(x => x.SendEvent(It.IsAny<IEvent>()), Times.Never);
        }

        [TestMethod]
        public void MessagesProcessor_OneMessageFailed_OtherShouldSend()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var saleId2 = new Guid("4832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId2 = new Guid("50E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var productKey2 = $"{itemId2}_{saleId2}_{countryId}";
            var encoding = new UTF8Encoding();

            var message1 = new Message { ItemId = itemId1, EventAction = "Create", IntegrationMessageId = Guid.NewGuid() };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId2, EventAction = "Create", IntegrationMessageId = Guid.NewGuid() };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var exceptionMessage = "It`s custom error message";
            var innerExceptionMessage = "It`s custom inner exception message";
            ProductService.Setup(x => x.GetProductInCountries(itemId1, null, null)).Throws(new Exception(exceptionMessage, new Exception(innerExceptionMessage)));
            ProductService.Setup(x => x.GetProductInCountries(itemId2, null, null)).Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey2, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()));
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object };
            var consumers = new List<ProductCreateConsumer> { new ProductCreateConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductCreateConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.AtLeast(2));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        [TestMethod]
        public void MessagesProcessor_ItemMessageWithDifferentItemId_ProcessedBothMessage()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var saleId2 = new Guid("B590900F-EF71-45BD-89E4-BF80D17DC397");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var itemId2 = new Guid("A960D459-F52A-4BE0-9651-AFECC7CCF3DA");
            var countryId = "AS";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var productKey2 = $"{itemId2}_{saleId2}_{countryId}";
            var encoding = new UTF8Encoding();
            var adminEventId1 = Guid.NewGuid();
            var adminEventId2 = Guid.NewGuid();

            var message1 = new Message { ItemId = itemId1, SaleId = saleId1, EventAction = "Change", IntegrationMessageId = adminEventId1 };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId2, EventAction = "Change", IntegrationMessageId = adminEventId2 };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();

            ProductService.Setup(x => x.GetProductInCountries(itemId1, It.IsAny<Guid?>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductService.Setup(x => x.GetProductInCountries(itemId2, It.IsAny<Guid?>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey2, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object, ProductEventSender2.Object };
            var consumers = new List<ProductChangeConsumer> { new ProductChangeConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductChangeConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 2);
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey2));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId1)));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId2)));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Exactly(2));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(1));
        }

        [TestMethod]
        public void MessagesProcessor_DoubledItemMessageWithSameItemIdAndSaleIdIsNull_ProcessedOnlyOneMessage()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var encoding = new UTF8Encoding();
            var adminEventId1 = Guid.NewGuid();
            var adminEventId2 = Guid.NewGuid();

            var message1 = new Message { ItemId = itemId1, SaleId = saleId1, EventAction = "Change", IntegrationMessageId = adminEventId1 };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId1, EventAction = "Change", IntegrationMessageId = adminEventId2 };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();

            ProductService.Setup(x => x.GetProductInCountries(itemId1, null, It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object, ProductEventSender2.Object };
            var consumers = new List<ProductChangeConsumer> { new ProductChangeConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductChangeConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 1);
            Assert.IsTrue(sender1Events.FirstOrDefault().Id == productKey1);
            Assert.IsTrue(sender1Events.FirstOrDefault().AdminEventIds.Contains(adminEventId1));
            Assert.IsTrue(sender1Events.FirstOrDefault().AdminEventIds.Contains(adminEventId2));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Once);
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        [TestMethod]
        public void MessagesProcessor_DoubledItemMessageWithSameItemIdAndSaleIdNotNull_ProcessedOnlyOneMessage()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "AS";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var encoding = new UTF8Encoding();
            var adminEventId1 = Guid.NewGuid();
            var adminEventId2 = Guid.NewGuid();

            var message1 = new Message { ItemId = itemId1, EventAction = "Change", IntegrationMessageId = adminEventId1 };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId1, SaleId = saleId1, EventAction = "Change", IntegrationMessageId = adminEventId2 };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();

            ProductService.Setup(x => x.GetProductInCountries(itemId1, null, It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()))
                .Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object, ProductEventSender2.Object };
            var consumers = new List<ProductChangeConsumer> { new ProductChangeConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductChangeConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 1);
            Assert.IsTrue(sender1Events.FirstOrDefault().Id == productKey1);
            Assert.IsTrue(sender1Events.FirstOrDefault().AdminEventIds.Contains(adminEventId1));
            Assert.IsTrue(sender1Events.FirstOrDefault().AdminEventIds.Contains(adminEventId2));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Once);
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        [TestMethod]
        public void MessagesProcessor_DoubledItemMessageWithSameItemIdAndDifferentSaleIds_ProcessedBothMessage()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var saleId2 = new Guid("4832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var countryId = "AS";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var productKey2 = $"{itemId1}_{saleId2}_{countryId}";
            var encoding = new UTF8Encoding();
            var adminEventId1 = Guid.NewGuid();
            var adminEventId2 = Guid.NewGuid();

            var message1 = new Message { ItemId = itemId1, SaleId = saleId1, EventAction = "Change", IntegrationMessageId = adminEventId1 };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId1, SaleId = saleId2, EventAction = "Change", IntegrationMessageId = adminEventId2 };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>())).Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId1, It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId2, It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey2, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object };
            var consumers = new List<ProductChangeConsumer> { new ProductChangeConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductChangeConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 2);
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey2));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId1)));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId2)));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Exactly(2));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(1));
        }

        [TestMethod]
        public void MessagesProcessor_DoubledItemMessageWithSameItemIdAndSaleIdDifferentCountry_ProcessedBothMessage()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId1 = "as";
            var countryId2 = "da";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId1}";
            var productKey2 = $"{itemId1}_{saleId1}_{countryId2}";
            var encoding = new UTF8Encoding();
            var adminEventId1 = Guid.NewGuid();
            var adminEventId2 = Guid.NewGuid();

            var message1 = new Message { ItemId = itemId1, SaleId = saleId1, CountryId = countryId1, EventAction = "Change", IntegrationMessageId = adminEventId1 };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId1, SaleId = saleId1, CountryId = countryId2, EventAction = "Change", IntegrationMessageId = adminEventId2 };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>())).Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId1, countryId1))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId1 } }));
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId1, countryId2))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey2, CountryId = countryId2 } }));
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object };
            var consumers = new List<ProductChangeConsumer> { new ProductChangeConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductChangeConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 2);
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey2));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId1)));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId2)));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Exactly(2));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(1));
        }

        [TestMethod]
        public void MessagesProcessor_DoubledItemMessageWithSameItemIdAndSaleIdFirstCountryEmpty_ProcessedOneMessage()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "as";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var encoding = new UTF8Encoding();
            var adminEventId1 = Guid.NewGuid();
            var adminEventId2 = Guid.NewGuid();

            var message1 = new Message { ItemId = itemId1, SaleId = saleId1, EventAction = "Change", IntegrationMessageId = adminEventId1 };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId1, SaleId = saleId1, CountryId = countryId, EventAction = "Change", IntegrationMessageId = adminEventId2 };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>())).Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId1, countryId))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId1, null))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object };
            var consumers = new List<ProductChangeConsumer> { new ProductChangeConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductChangeConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 1);
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId1)));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId2)));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Exactly(1));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(1));
        }

        [TestMethod]
        public void MessagesProcessor_DoubledItemMessageWithSameItemIdAndSaleIdDuplicateCountryEmpty_ProcessedOneMessage()
        {
            ResetCalls();

            var saleId1 = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId1 = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "as";
            var productKey1 = $"{itemId1}_{saleId1}_{countryId}";
            var encoding = new UTF8Encoding();
            var adminEventId1 = Guid.NewGuid();
            var adminEventId2 = Guid.NewGuid();

            var message1 = new Message { ItemId = itemId1, SaleId = saleId1, CountryId = countryId, EventAction = "Change", IntegrationMessageId = adminEventId1 };
            var eventData1 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message1).ToLower()));
            eventData1.Properties.Add("Type", message1.EventAction);

            var message2 = new Message { ItemId = itemId1, SaleId = saleId1, EventAction = "Change", IntegrationMessageId = adminEventId2 };
            var eventData2 = new EventData(encoding.GetBytes(JsonConvert.SerializeObject(message2).ToLower()));
            eventData2.Properties.Add("Type", message2.EventAction);

            var sender1Events = new List<IEvent>();
            ProductEventSender1.Setup(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>())).Callback((IEnumerable<IEvent> productEvents) => { sender1Events.AddRange(productEvents); });
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId1, countryId))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductService.Setup(x => x.GetProductInCountries(itemId1, saleId1, null))
                .Returns(Task.FromResult<IEnumerable<ProductInCountry>>(new List<ProductInCountry> { new ProductInCountry { Id = productKey1, CountryId = countryId } }));
            ProductEventSender1.Setup(x => x.CanSend(It.IsAny<IEvent>())).Returns(true);

            var senders = new List<IProductEventSender> { ProductEventSender1.Object };
            var consumers = new List<ProductChangeConsumer> { new ProductChangeConsumer(ProductService.Object) };

            var service = new MessagesProcessor<ProductChangeConsumer, IProductEventSender>(consumers, senders);

            service.ProcessEventsAsync(new PartitionContext(), new List<EventData> { eventData1, eventData2 }).ConfigureAwait(true);

            Assert.AreEqual(sender1Events.Count, 1);
            Assert.IsTrue(sender1Events.Any(t => t.Id == productKey1));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId1)));
            Assert.IsTrue(sender1Events.Any(t => t.AdminEventIds.Contains(adminEventId2)));
            ProductService.Verify(x => x.GetProductInCountries(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Exactly(1));
            ProductEventSender1.Verify(x => x.SendEvents(It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(1));
        }

        private void ResetCalls()
        {
            ProductService.ResetCalls();
            ProductEventSender1.ResetCalls();
            ProductEventSender2.ResetCalls();
            ProductEventConsumer.ResetCalls();
        }
    }
}
