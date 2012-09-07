namespace NserviceBus.Instrumentation.Providers.Saga
{
	public class SagaData
	{
		public string MachineName { get; set; }
		public string SagaId { get; set; }
		public string Data { get; set; }


		public string SagaType { get; set; }

		public string ServiceName { get; set; }
	}
}