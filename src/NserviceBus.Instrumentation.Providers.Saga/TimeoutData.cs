using System;
using System.Collections.Generic;

namespace NserviceBus.Instrumentation.Providers.Saga
{
	public class TimeoutData
	{
		public string MachineName { get; set; }
		public string SagaId { get; set; }
		public string Data { get; set; }
		public string TimeoutState { get; set; }
		public string DocumentId { get; set; }
		public DateTime ExpiresUtc { get; set; }
		public string ServiceName { get; set; }
		public Guid TimeoutDataId { get; set; }
		public IEnumerable<KeyValuePair<string, string>> MessageDictionary { get; set; }
	}
}