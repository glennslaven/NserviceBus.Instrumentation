using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using Formatting = Newtonsoft.Json.Formatting;

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

					var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data, new JsonSerializerSettings
						{
						Error = (sender, args) => { args.ErrorContext.Handled = true; }
					}) as Dictionary<string, string>;

					returnVal.Add(new SagaData
						{
							Data = data,
							MachineName = machineName,
							SagaId = sagaId,
							ServiceName = serviceName,
							SagaType = sagaType,
							SagaDictionary = dic
						});					
				}				
			}
			catch (WebException ex)
			{
				Logger.Error(string.Format("Error loading {0}", url), ex);
			}
			catch (JsonSerializationException ex)
			{
				Logger.Error(string.Format("parsing JSON {0}", json), ex);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}

			return returnVal;
		}

		public List<TimeoutData> GetTimeoutData(string serviceName)
		{
			var client = new WebClient();
			var url = string.Format(SagaDocumentFormat, serviceName, "TimeoutData");
			var json = string.Empty;
			var returnVal = new List<TimeoutData>();
			try
			{
				json = client.DownloadString(url);

				var timeoutData = JObject.Parse(json) as dynamic;

				Logger.InfoFormat("{0} {1} timeouts found", timeoutData["TotalResults"], "TimeoutData");

				foreach (var timeout in timeoutData.Results)
				{
					var sagaId = timeout.SagaId;
					var machineName = timeout.Destination.Machine;
					var expiresUtc = timeout.Time.Value;
					var state = Encoding.ASCII.GetString(Convert.FromBase64String(timeout.State.Value));

					var doc = new XmlDocument();
					doc.LoadXml(state);

					var jsonText = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);
					var timeoutmessage = JObject.Parse(jsonText) as dynamic;
					var messageKey = ((IDictionary<String, JToken>)timeoutmessage.Messages).Keys.First(k => !k.StartsWith("@xml"));
					var message = (timeoutmessage.Messages[messageKey] as IDictionary<String, JToken>).Select(i => new KeyValuePair<string, string>(i.Key, i.Value.ToString()));

					returnVal.Add(new TimeoutData
						{
							ServiceName = serviceName,
							Data = timeout.ToString(),
							DocumentId = timeout["@metadata"]["@id"],
							ExpiresUtc = expiresUtc,
							MachineName = machineName,
							SagaId = sagaId,
							TimeoutState = state,
							MessageDictionary = message

						});
				}
			}
			catch (WebException)
			{
				//Logger.Error(string.Format("Error loading {0}", url), ex);
				//throw;
			}
			catch (JsonSerializationException ex)
			{
				Logger.Error(string.Format("parsing JSON {0}", json), ex);
			}
			catch(Exception ex)
			{
				Logger.Error(ex);
			}

			return returnVal;
		}
	}

	public interface IRavenSagaProvider
	{
		List<string> GetSagaTypes(string serviceName);
		List<SagaData> GetSagaData(string sagaType, string serviceName);
		List<TimeoutData> GetTimeoutData(string serviceName);
	}
}
