using System;
using System.Collections.Generic;

namespace NServiceBus.Instrumentation.Providers.Saga
{
	public class SagaData
	{
		public Guid SagaDataId { get; set; }
		public string MachineName { get; set; }
		public string SagaId { get; set; }
		public string Data { get; set; }
		public string SagaType { get; set; }
		public string ServiceName { get; set; }
		public Dictionary<string, string> SagaDictionary { get; set; }
	}
}