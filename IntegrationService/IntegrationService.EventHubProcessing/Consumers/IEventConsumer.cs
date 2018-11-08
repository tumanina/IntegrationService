using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationService.EventHubProcessing.Entities;

namespace IntegrationService.EventHubProcessing.Consumers
{
    public interface IEventConsumer
    {
        bool CanConsume(Message message);

        Task<IEnumerable<IEvent>> Consume(Message message);
    }
}
