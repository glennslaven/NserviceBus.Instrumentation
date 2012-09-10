using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NserviceBus.Instrumentation.Interfaces;
using log4net;

namespace NserviceBus.Instrumentation.Providers.Saga
{
	internal class SagaInstrumentationProvider : ISagaInstrumentationProvider
	{
		private IEnumerable<string> ServiceNames { get; set; }

		public ILog Logger { get; set; }
		public IRavenSagaProvider RavenSagaProvider { get; set; }
		public ConnectionStringSettings ConnectionStringSettings { get; set; }

		private DbConnection GetConnection()
		{
			var connection = new SqlConnection(ConnectionStringSettings.ConnectionString);
			connection.Open();

			return connection;
		}

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
					data.ForEach(d => d.SagaDataId = Guid.NewGuid());

					using (var connection = GetConnection())
					{
						var insertParams = data.Select(i => new { i.SagaDataId, i.SagaType, i.SagaId, i.ServiceName, i.MachineName, i.Data });
						connection.Execute("SagaData_Insert", insertParams, commandType: CommandType.StoredProcedure);

						foreach (var sagaData in data)
						{
							var sagaDataParam = sagaData.SagaDictionary.Select(i => new { sagaData.SagaDataId, KeyName = i.Key, KeyValue = i.Value }).ToList();
							connection.Execute("INSERT INTO SagaDataValues (SagaDataId, KeyName, KeyValue) VALUES (@SagaDataId, @KeyName, @KeyValue)", sagaDataParam, commandType: CommandType.Text);
						}
					}
				}

				var timeouts = RavenSagaProvider.GetTimeoutData(serviceName);
				timeouts.ForEach(t => t.TimeoutDataId = Guid.NewGuid());

				if (timeouts.Count > 0)
				{
					using (var connection = GetConnection())
					{
						var timeoutParams = timeouts.Select(i => new { i.TimeoutDataId, i.DocumentId, i.SagaId, i.ServiceName, i.MachineName, i.TimeoutState, i.Data, i.ExpiresUtc });
						connection.Execute("TimeoutData_Insert", timeoutParams, commandType: CommandType.StoredProcedure);

						foreach (var timeoutData in timeouts)
						{
							var timeoutDataParam = timeoutData.MessageDictionary.Select(i => new { timeoutData.TimeoutDataId, KeyName = i.Key, KeyValue = i.Value }).ToList();
							connection.Execute("INSERT INTO TimeoutDataValues (TimeoutDataId, KeyName, KeyValue) VALUES (@TimeoutDataId, @KeyName, @KeyValue)", timeoutDataParam, commandType: CommandType.Text);
						}
					}
				}
			}
		}
	}

	public interface ISagaInstrumentationProvider : IInstrumentationProvider
	{
		
	}
}
