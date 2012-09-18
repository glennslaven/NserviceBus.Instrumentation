using System.Collections.Generic;

namespace NServiceBus.Instrumentation.Dashboard.Models.DataProviders.Home
{
	public class IndexDataModel
	{
		public List<Machine> Machines { get; set; }

		public class Machine
		{
			public string Name { get; set; }
			public List<Service> Services { get; set; }

			public class Service
			{
				public string Name { get; set; }
			}
		}
	}
}