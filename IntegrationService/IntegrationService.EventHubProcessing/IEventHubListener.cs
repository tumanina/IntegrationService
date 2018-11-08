using System.Threading.Tasks;

namespace IntegrationService.EventHubProcessing
{
    public interface IEventHubListener
    {
        Task Register();
        Task Unregister();
    }
}
