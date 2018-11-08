using System.Configuration;

namespace IntegrationService.API.ConfigurationSections
{
    public class EventsSection : ConfigurationSection
    {
        [ConfigurationProperty("senders", IsDefaultCollection = true)]
        public Senders Senders
        {
            get
            {
                return (Senders)this["senders"];
            }
            set
            {
                this["senders"] = (object)value;
            }
        }
    }

    [ConfigurationCollection(typeof(Sender))]
    public class Senders : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return (ConfigurationElement)new Sender();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Sender)(element)).CountryIds;
        }

        public Sender this[int idx]
        {
            get { return (Sender)BaseGet(idx); }
        }
    }

    public class Sender : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)this["name"]); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("countryIds", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string CountryIds
        {
            get { return ((string)this["countryIds"]); }
            set { this["countryIds"] = value; }
        }

        [ConfigurationProperty("productConnectionString", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string ProductConnectionString
        {
            get { return ((string)(this["productConnectionString"])); }
            set { this["productConnectionString"] = value; }
        }

        [ConfigurationProperty("skuConnectionString", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string SkuConnectionString
        {
            get { return ((string)(this["skuConnectionString"])); }
            set { this["skuConnectionString"] = value; }
        }

        [ConfigurationProperty("saleConnectionString", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string SaleConnectionString
        {
            get { return ((string)(this["saleConnectionString"])); }
            set { this["saleConnectionString"] = value; }
        }
    }
}