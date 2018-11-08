namespace IntegrationService.EventHubProcessing.Entities.Events
{
    public class SkuEvent : BaseInnerEvent
    {
        public SkuEvent() : base(EventType.Sku)
        {
        }
    }
}
