using System;
using System.Configuration;

namespace IntegrationService.API.ConfigurationSections
{
    public class TaxonomySection : ConfigurationSection
    {
        [ConfigurationProperty("trees", IsDefaultCollection = true)]
        public TaxonomyTrees TaxonomyTrees
        {
            get
            {
                return (TaxonomyTrees)this["trees"];
            }
            set
            {
                this["trees"] = (object)value;
            }
        }
    }

    [ConfigurationCollection(typeof(TaxonomyTrees))]
    public class TaxonomyTrees : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return (ConfigurationElement)new TaxonomyTree();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TaxonomyTree)(element)).CountryId;
        }

        public TaxonomyTree this[int idx]
        {
            get { return (TaxonomyTree)BaseGet(idx); }
        }
    }

    public class TaxonomyTree : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)this["name"]); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("countryId", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string CountryId
        {
            get { return ((string)this["countryId"]); }
            set { this["countryId"] = value; }
        }

        [ConfigurationProperty("treeId", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string TreeId
        {
            get { return ((string)(this["treeId"])); }
            set { this["treeId"] = value; }
        }
    }
}