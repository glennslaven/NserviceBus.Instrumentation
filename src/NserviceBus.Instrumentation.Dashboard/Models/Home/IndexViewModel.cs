using System.Collections.Generic;
using System.Web.Mvc;

namespace NserviceBus.Instrumentation.Dashboard.Models.Home
{
	public class IndexViewModel
	{
		public List<Machine> Machines { get; set; }
		public MvcHtmlString JsonModel { get; set; }

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