using System.Collections.Generic;
using System.Linq;
using System.Net;
using NserviceBus.Instrumentation.Interfaces;
using log4net;

namespace NserviceBus.Instrumentation.Providers.Saga
{
	internal class SagaInstrumentationProvider : ISagaInstrumentationProvider
	{
		private IEnumerable<string> ServiceNames { get; set; }

		public ILog Logger { get; set; }
		public IRavenSagaProvider RavenSagaProvider { get; set; }

		public void Setup(IEnumerable<string> serviceNames)
		{
			ServiceNames = serviceNames;
		}		

		public void Instrument()
		{
			foreach (var serviceName in ServiceNames)
			{
				var sagaTypes = RavenSagaProvider.GetSagaTypes(serviceName);

				foreach (var sagaType in sagaTypes)
				{
					var data = RavenSagaProvider.GetSagaData(sagaType, serviceName);
				}
			}
		}
	}

	public interface ISagaInstrumentationProvider : IInstrumentationProvider
	{
		
	}
}
