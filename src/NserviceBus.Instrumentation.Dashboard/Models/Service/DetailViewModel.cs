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
		public MvcHtmlString JsonModel { get; set; }

		public class SagaClass
		{
			public SagaClass()
			{
				Values = new List<KeyValueClass>();
			}

			public Guid SagaDataId { get; set; }
			public Guid SagaId { get; set; }
			public string SagaType { get; set; }
			public string MachineName { get; set; }
			public string ServiceName { get; set; }
			public List<KeyValueClass> Values { get; set; }

		}

		public class KeyValueClass
		{
			public string KeyName { get; set; }
			public string KeyValue { get; set; }
		}
	}
}