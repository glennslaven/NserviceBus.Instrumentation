using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NserviceBus.Instrumentation.Dashboard.Models.Service
{
	public class DetailViewModel
	{
		public List<SagaClass> Sagas { get; set; }

		public string MachineName { get; set; }
		public string ServiceName { get; set; }

		public class SagaClass
		{
			public SagaClass()
			{
				Values = new Dictionary<string, string>();
				Timeouts = new List<TimeoutClass>();
			}

			public Guid SagaDataId { get; set; }
			public Guid SagaId { get; set; }
			public string SagaType { get; set; }
			public string MachineName { get; set; }
			public string ServiceName { get; set; }
			public Dictionary<string, string> Values { get; set; }
			public List<TimeoutClass> Timeouts { get; set; }

		}

		public class TimeoutClass
		{
			public TimeoutClass()
			{
				Values = new Dictionary<string, string>();
			}

			public Guid TimeoutDataId { get; set; }
			public DateTime ExpiresUtc { get; set; }
			public Guid SagaId { get; set; }
			public Dictionary<string, string> Values { get; set; }
		}

		public MvcHtmlString JsonModel { get; set; }
	}
}