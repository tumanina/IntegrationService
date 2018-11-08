using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Consumers;
using Microsoft.ServiceBus.Messaging;

namespace IntegrationService.EventHubProcessing
{
    public class EventHubListener<TConsumer, TEventSender> : IEventHubListener
        where TConsumer : IEventConsumer
        where TEventSender : IEventSender
    {
        private EventProcessorHost _eventProcessorHost;
        private readonly string _eventHubPath;
        private readonly string _eventHubConnectionString;
        private readonly string _storageConnectionString;
        private readonly string _eventHubConsumerGroup;
        private readonly IEnumerable<TConsumer> _consumers;
        private readonly IEnumerable<TEventSender> _eventSenders;

        public EventHubListener(string eventHubPath, string eventHubConnectionString, string storageConnectionString, string eventHubConsumerGroup, 
            IEnumerable<TConsumer> consumers, IEnumerable<TEventSender> eventSenders)
        {
            _eventHubPath = eventHubPath;
            _eventHubConnectionString = eventHubConnectionString;
            _storageConnectionString = storageConnectionString;
            _eventHubConsumerGroup = eventHubConsumerGroup;
            _consumers = consumers;
            _eventSenders = eventSenders;
        }

        public async Task Register()
        {
            _eventProcessorHost = new EventProcessorHost(_eventHubPath, _eventHubConsumerGroup, _eventHubConnectionString, _storageConnectionString);

            await _eventProcessorHost.RegisterEventProcessorFactoryAsync(new MessagesProcessorFactory<TConsumer, TEventSender>(_eventSenders, _consumers));
        }

        public async Task Unregister()
        {
            await _eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}