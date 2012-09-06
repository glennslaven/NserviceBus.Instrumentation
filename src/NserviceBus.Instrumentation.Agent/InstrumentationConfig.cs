using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace NserviceBus.Instrumentation.Agent
{
	public class InstrumentationConfig : ConfigurationSection
	{
		public static InstrumentationConfig Config { get; set; }

		static InstrumentationConfig()
		{
			Config = ConfigurationManager.GetSection("instrumentationConfig") as InstrumentationConfig; 
		}

		[ConfigurationProperty("providers", IsDefaultCollection = false, IsKey = false, IsRequired = true)]
		public ProviderCollection Providers
		{
			get { return base["providers"] as ProviderCollection; }
			set { base["providers"] = value; }
		}

		[ConfigurationProperty("services", IsDefaultCollection = false, IsKey = false, IsRequired = false)]
		public ServicesConfig Services
		{
			get { return base["services"] as ServicesConfig; }
			set { base["services"] = value; }
		}

		[ConfigurationCollection(typeof(Provider), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
		public class ProviderCollection : ConfigurationElementCollection
		{
			protected override ConfigurationElement CreateNewElement()
			{
				return new Provider();
			}

			protected override ConfigurationElement CreateNewElement(string elementName)
			{
				return new Provider{Name = elementName};
			}

			protected override object GetElementKey(ConfigurationElement element)
			{
				return ((Provider)element).Name;
			}

			protected override string ElementName
			{
				get { return "provider"; }
			}

			protected override bool IsElementName(string elementName)
			{
				return !String.IsNullOrEmpty(elementName) && elementName == "provider";
			}

			public Provider this[int index]
			{
				get { return BaseGet(index) as Provider; }
			}
		}

		[DebuggerDisplay("{Name}")]
		public class Provider : ConfigurationElement
		{
			[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
			public string Name
			{
				get { return (string)this["name"]; }
				set { this["name"] = value; }
			}
		}

		public class ServicesConfig : ConfigurationElement
		{
			[ConfigurationProperty("autoload", DefaultValue = true)]
			public bool Autoload
			{
				get { return base["autoload"] is bool && (bool)base["autoload"]; }
				set { base["autoload"] = value; }
			}

			[ConfigurationProperty("whitelist", IsDefaultCollection = false, IsKey = false, IsRequired = false)]
			public ServiceCollection Whitelist
			{
				get { return base["whitelist"] as ServiceCollection; }
				set { base["whitelist"] = value; }
			}

			[ConfigurationProperty("blacklist", IsDefaultCollection = false, IsKey = false, IsRequired = false)]
			public ServiceCollection Blacklist
			{
				get { return base["blacklist"] as ServiceCollection; }
				set { base["blacklist"] = value; }
			}

			[ConfigurationProperty("", IsDefaultCollection = true, IsKey = false, IsRequired = false)]
			public ServiceCollection ExplicitList
			{
				get { return base[""] as ServiceCollection; }
				set { base[""] = value; }
			}

			protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
			{
				base.DeserializeElement(reader, serializeCollectionKey);

				if (Whitelist.Count > 0 & Blacklist.Count > 0)
				{
					throw new ConfigurationErrorsException("You cannot specify both blacklist & whitelist");
				}
			}
		}

		[ConfigurationCollection(typeof(Service), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
		public class ServiceCollection : ConfigurationElementCollection
		{
			protected override ConfigurationElement CreateNewElement()
			{
				return new Service();
			}

			protected override object GetElementKey(ConfigurationElement element)
			{
				return ((Service)element).Name;
			}

			protected override string ElementName
			{
				get { return "service"; }
			}

			protected override bool IsElementName(string elementName)
			{
				return !String.IsNullOrEmpty(elementName) && elementName == "service";
			}

			public Service this[int index]
			{
				get { return BaseGet(index) as Service; }
			}

			public bool Contains(string name)
			{
				return BaseGetAllKeys().ToList().Contains(name);
			}
		}

		[DebuggerDisplay("{Name}")]
		public class Service : ConfigurationElement
		{
			[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
			public string Name
			{
				get { return (string)this["name"]; }
				set { this["name"] = value; }
			}
		}
	}
}
