using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NserviceBus.Instrumentation.Interfaces;
using log4net;

namespace NserviceBus.Instrumentation.Providers.Saga
{
	internal class SagaInstrumentationProvider : ISagaInstrumentationProvider
	{
		private const string DocumentNameFormat = "http://localhost:8080/databases/{0}/terms/Raven/DocumentsByEntityName?field=Tag&fromValue=&pageSize=100&noCache=774278217";
		private const string SagaDocumentFormat = "http://localhost:8080/databases/{0}/indexes/Raven/DocumentsByEntityName?query=Tag%253A{1}&start=0&pageSize=100&aggregation=None&noCache=-715965163";

		
		private IEnumerable<string> ServiceNames { get; set; }

		public ILog Logger { get; set; }

		public void Setup(IEnumerable<string> serviceNames)
		{
			ServiceNames = serviceNames;
		}		

		public void Instrument()
		{
			var client = new WebClient();

			foreach (var serviceName in ServiceNames)
			{
				var url = string.Format(DocumentNameFormat, serviceName);
				try
				{
					var json = client.DownloadString(url);

					var types = JsonConvert.DeserializeObject<List<string>>(json);

					var sagaTypes = types.Where(t => t.EndsWith("Saga")).ToList();

					foreach (var sagaType in sagaTypes)
					{
						url = string.Format(SagaDocumentFormat, serviceName, sagaType);
						json = client.DownloadString(url);

						var sagaData = JObject.Parse(json);

						Logger.InfoFormat("{0} {1} sagas found", sagaData["TotalResults"], sagaType);
					}
				}
				catch (WebException ex)
				{
					
				}
				catch(JsonSerializationException ex)
				{
					
				}
			}
		}
	}

	public interface ISagaInstrumentationProvider : IInstrumentationProvider
	{
		
	}
}
