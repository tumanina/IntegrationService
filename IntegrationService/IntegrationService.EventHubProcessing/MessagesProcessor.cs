using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApacKernel.AspNet;
using ApacKernel.Diagnostics;
using IntegrationService.EventHubProcessing.Consumers;
using IntegrationService.EventHubProcessing.Entities;
using Microsoft.ServiceBus.Messaging;

namespace IntegrationService.EventHubProcessing
{
    public class MessagesProcessor<TConsumer, TEventSender> : IEventProcessor
        where TConsumer : IEventConsumer
        where TEventSender : IEventSender
    {
        private const string EVENT_TYPE_PROPERTY_NAME = "Type";
        private readonly Log _log = Log.GetLogger<MessagesProcessor<TConsumer, TEventSender>>();
        private readonly IEnumerable<TEventSender> _eventSenders;
        private readonly IEnumerable<TConsumer> _consumers;

        public MessagesProcessor(IEnumerable<TConsumer> consumers, IEnumerable<TEventSender> eventSenders)
        {
            _eventSenders = eventSenders;
            _consumers = consumers;
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> eventsData)
        {
            var tasks = new List<Task<IEnumerable<IEvent>>>();

            foreach (var eventData in eventsData)
            {
                if (!eventData.Properties.Keys.Contains(EVENT_TYPE_PROPERTY_NAME))
                {
                    continue;
                }

                var eventAction = eventData.Properties[EVENT_TYPE_PROPERTY_NAME].ToString();
                var message = Json.FromJson<Message>(Encoding.UTF8.GetString(eventData.GetBytes()));
                message.EventAction = eventAction;
                message.AdminEventIds = new List<Guid> { message.IntegrationMessageId };

                tasks.Add(GetEvents(message));
            }

            var events = new List<IEvent>();

            foreach (var result in await Task.WhenAll(tasks.ToArray()))
            {
                events.AddRange(result);
            }

            foreach (var sender in _eventSenders)
            {
                try
                {
                    var sendingEvents = events.Where(t => sender.CanSend(t)).ToList();

                    if (sendingEvents.Any())
                    {
                        sender.SendEvents(sendingEvents);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }

            await context.CheckpointAsync();
        }

        private async Task<IEnumerable<IEvent>> GetEvents(Message message)
        {
            var consumer = _consumers.FirstOrDefault(t => t.CanConsume(message));

            try
            {
                if (consumer != null)
                {
                    return await consumer.Consume(message);
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            return new List<IEvent>();
        }
    }
}
