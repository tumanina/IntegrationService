
namespace IntegrationService.EventHubProcessing.Entities.Events
{
    public class ProductEvent : BaseInnerEvent
    {
        public ProductEvent() : base(EventType.Product)
        {
        }
    }
}
