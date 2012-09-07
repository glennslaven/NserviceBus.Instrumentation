using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

namespace NserviceBus.Instrumentation.Providers.Saga
{
	internal class RavenSagaProvider : IRavenSagaProvider
	{
		private const string DocumentNameFormat = "http://localhost:8080/databases/{0}/terms/Raven/DocumentsByEntityName?field=Tag&fromValue=&pageSize=100&noCache=774278217";
		private const string SagaDocumentFormat = "http://localhost:8080/databases/{0}/indexes/Raven/DocumentsByEntityName?query=Tag%253A{1}&start=0&pageSize=100&aggregation=None&noCache=-715965163";

		public ILog Logger { get; set; }

		public List<string> GetSagaTypes(string serviceName)
		{
			var client = new WebClient();

			var url = string.Format(DocumentNameFormat, serviceName);
			var json = string.Empty;
			var sagaTypes = new List<string>();
			try
			{
				json = client.DownloadString(url);

				var types = JsonConvert.DeserializeObject<List<string>>(json);

				sagaTypes = types.Where(t => t.EndsWith("Saga")).ToList();				
			}
			catch (WebException) { }
			catch (JsonSerializationException ex)
			{
				Logger.Error(string.Format("parsing JSON {0}", json), ex);
			}

			return sagaTypes; 
		}

		public dynamic GetSagaData(string sagaType, string serviceName)
		{
			var client = new WebClient();

			var url = string.Format(SagaDocumentFormat, serviceName, sagaType);
			var json = string.Empty;

			try
			{
				json = client.DownloadString(url);

				var sagaData = JObject.Parse(json);

				Logger.InfoFormat("{0} {1} sagas found", sagaData["TotalResults"], sagaType);

				return sagaData;
			}
			catch (WebException ex)
			{
				Logger.Error(string.Format("Error loading {0}", url), ex);
				throw;
			}
			catch (JsonSerializationException ex)
			{
				Logger.Error(string.Format("parsing JSON {0}", json), ex);
				throw;
			}
		}
	}

	public interface IRavenSagaProvider
	{
		List<string> GetSagaTypes(string serviceName);
		dynamic GetSagaData(string sagaType, string serviceName);
	}
}
