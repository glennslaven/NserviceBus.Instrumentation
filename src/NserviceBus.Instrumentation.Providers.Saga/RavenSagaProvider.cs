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

				url = string.Format(SagaDocumentFormat, serviceName, "SagaUniqueIdentity");
				json = client.DownloadString(url);

				sagaUniqueIds = JObject.Parse(json) as dynamic;
			}
			catch (WebException) { }
			catch (JsonSerializationException ex)
			{
				Logger.Error(string.Format("parsing JSON {0}", json), ex);
			}

			return sagaTypes; 
		}

		public List<SagaData> GetSagaData(string sagaType, string serviceName)
		{
			var client = new WebClient();

			var url = string.Format(SagaDocumentFormat, serviceName, sagaType);
			var json = string.Empty;
			var returnVal = new List<SagaData>();
			try
			{
				json = client.DownloadString(url);

				var sagaData = JObject.Parse(json) as dynamic;

				Logger.InfoFormat("{0} {1} sagas found", sagaData["TotalResults"], sagaType);

				foreach (var saga in sagaData.Results)
				{
					var sagaId = saga["@metadata"]["@id"].Value.Replace(sagaType.ToLower() + "/", "");
					var originator = saga.Originator;
					var machineName = originator.Value.Split(new[] { '@' })[1];
					var data = saga.ToString();

					returnVal.Add(new SagaData
						{
							Data = data,
							MachineName = machineName,
							SagaId = sagaId,
							ServiceName = serviceName,
							SagaType = sagaType
						});					
				}

				return returnVal;
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
		List<SagaData> GetSagaData(string sagaType, string serviceName);
	}
}
