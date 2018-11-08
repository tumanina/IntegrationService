using System.Collections.Generic;
using IntegrationService.EventHubProcessing.Consumers;
using Microsoft.ServiceBus.Messaging;

namespace IntegrationService.EventHubProcessing
{
    public class MessagesProcessorFactory<TConsumer, TEventSender> : IEventProcessorFactory
        where TConsumer : IEventConsumer
        where TEventSender : IEventSender
    {
        private readonly IEnumerable<TEventSender> _eventSenders;
        private readonly IEnumerable<TConsumer> _consumers;

        public MessagesProcessorFactory(IEnumerable<TEventSender> eventSenders, IEnumerable<TConsumer> consumers)
        {
            _eventSenders = eventSenders;
            _consumers = consumers;
        }

        IEventProcessor IEventProcessorFactory.CreateEventProcessor(PartitionContext context)
        {
            return new MessagesProcessor<TConsumer, TEventSender>(_consumers, _eventSenders);
        }
    }
}
