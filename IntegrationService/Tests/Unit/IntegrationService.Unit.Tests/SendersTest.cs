using System;
using System.Collections.Generic;
using IntegrationService.EventHubProcessing;
using IntegrationService.EventHubProcessing.Entities;
using IntegrationService.EventHubProcessing.Entities.Events;
using IntegrationService.EventHubProcessing.Senders;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntegrationService.Unit.Tests
{
    [TestClass]
    public class SendersTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
        }

        [TestMethod]
        public void ProductEventsSenderTA_EventIsTopBuy_CanSend()
        {
            var eventHubSender = new Mock<IEventHubSender>();
            eventHubSender.Setup(x => x.Send(It.IsAny<EventData>()));
            var sender = new ProductEventSender(eventHubSender.Object, new List<string> { "TA" });

            var result = sender.CanSend(new ProductEvent { CountryId = "TA" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ProductEventsSenderTA_EventIsNotTopBuy_CanNotSend()
        {
            var eventHubSender = new Mock<IEventHubSender>();
            eventHubSender.Setup(x => x.Send(It.IsAny<EventData>()));
            var sender = new ProductEventSender(eventHubSender.Object, new List<string> { "TA" });

            var result = sender.CanSend(new ProductEvent { CountryId = "AS" });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ProductEventsSenderTAAndOO_EventIsTopBuy_ShouldSend()
        {
            var eventHubSender = new Mock<IEventHubSender>();
            eventHubSender.Setup(x => x.Send(It.IsAny<EventData>()));
            var sender = new ProductEventSender(eventHubSender.Object, new List<string> { "TA", "OO" });

            var result = sender.CanSend(new ProductEvent { CountryId = "TA" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ProductEventsSenderTAAndOO_EventIsOO_ShouldSend()
        {
            var eventHubSender = new Mock<IEventHubSender>();
            eventHubSender.Setup(x => x.Send(It.IsAny<EventData>()));
            var sender = new ProductEventSender(eventHubSender.Object, new List<string> { "TA", "OO" });

            var result = sender.CanSend(new ProductEvent { CountryId = "OO" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ProductEventsSenderTAAndOO_EventIsAS_NotSend()
        {
            var eventHubSender = new Mock<IEventHubSender>();
            eventHubSender.Setup(x => x.Send(It.IsAny<EventData>()));
            var sender = new ProductEventSender(eventHubSender.Object, new List<string> { "TA", "OO" });

            var result = sender.CanSend(new ProductEvent { CountryId = "AS" });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ProductEventsSenderAll_EventIsNotTopBuy_CanSend()
        {
            var eventHubSender = new Mock<IEventHubSender>();
            eventHubSender.Setup(x => x.Send(It.IsAny<EventData>()));
            var sender = new ProductEventSender(eventHubSender.Object, new List<string> {});

            var result = sender.CanSend(new ProductEvent { CountryId = "TA" });

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void ProductEventsSenderAll_EventIsTopBuy_CanSend()
        {
            var eventHubSender = new Mock<IEventHubSender>();
            eventHubSender.Setup(x => x.Send(It.IsAny<EventData>()));
            var sender = new ProductEventSender(eventHubSender.Object, new List<string> { });

            var result = sender.CanSend(new ProductEvent { CountryId = "AS" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ProductEventSender_SendReconciliationEvent_AddedParameterToEvent()
        {
            var saleId = new Guid("3832896B-3CF1-4B78-BAF8-22DBEA26B633");
            var itemId = new Guid("D0E8C5CB-E787-4B83-97FC-67C5A9AF6880");
            var countryId = "TA";
            var productKey = $"{itemId}_{saleId}_{countryId}";
            var sendedEvent = new EventData();

            var productEvent = new ProductEvent { Id = productKey, EventAction = EventAction.Updated };

            var EventHubSender = new Mock<IEventHubSender>();
            EventHubSender.Setup(x => x.Send(It.IsAny<EventData>())).Callback((EventData _eventData) => { sendedEvent = _eventData; });

            var sender = new ProductEventSender(EventHubSender.Object, new List<string> { "TA" });

            sender.SendRefeedEvent(productEvent);

            Assert.IsTrue(sendedEvent.Properties.Keys.Contains("EventType"));
        }
    }
}
