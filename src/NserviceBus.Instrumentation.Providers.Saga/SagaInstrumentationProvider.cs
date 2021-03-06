﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NServiceBus.Instrumentation.Interfaces;
using log4net;

namespace NServiceBus.Instrumentation.Providers.Saga
{
	internal class SagaInstrumentationProvider : ISagaInstrumentationProvider
	{
		private IEnumerable<IServiceInfo> Services { get; set; }

		public ILog Logger { get; set; }
		public IRavenSagaProvider RavenSagaProvider { get; set; }
		public ConnectionStringSettings ConnectionStringSettings { get; set; }

		private DbConnection GetConnection()
		{
			var connection = new SqlConnection(ConnectionStringSettings.ConnectionString);
			connection.Open();

			return connection;
		}

		public void Setup(IEnumerable<IServiceInfo> services)
		{
			Services = services;
		}

		public void Instrument()
		{
			using (var connection = GetConnection())
			{
				connection.Execute("UPDATE SagaData SET Archive = 1;");
			}

			foreach (var serviceName in Services.Select(s => s.Name))
			{
				var sagaTypes = RavenSagaProvider.GetSagaTypes(serviceName);
				foreach (var sagaType in sagaTypes)
				{
					var data = RavenSagaProvider.GetSagaData(sagaType, serviceName);
					data.ForEach(d => { if (d != null) d.SagaDataId = Guid.NewGuid(); });

					var insertParams = data.Select(i => new { i.SagaDataId, i.SagaType, i.SagaId, i.ServiceName, i.MachineName, i.Data });
					Parallel.ForEach(insertParams, p =>
						{
							using (var connection = GetConnection())
							{
								connection.Execute("SagaData_Insert", p, commandType: CommandType.StoredProcedure);
							}
						});

					Parallel.ForEach(data, sagaData =>
						{
							using (var connection = GetConnection())
							{
								var sagaDataParam = sagaData.SagaDictionary.Select(i => new { sagaData.SagaDataId, KeyName = i.Key, KeyValue = i.Value }).ToList();
								connection.Execute("INSERT INTO SagaDataValues (SagaDataId, KeyName, KeyValue) VALUES (@SagaDataId, @KeyName, @KeyValue)", sagaDataParam, commandType: CommandType.Text);
							}

						});

							

				}

				var timeouts = RavenSagaProvider.GetTimeoutData(serviceName);
				timeouts.ForEach(t => t.TimeoutDataId = Guid.NewGuid());

				if (timeouts.Count > 0)
				{
					var timeoutParams = timeouts.Select(i => new { i.TimeoutDataId, i.DocumentId, i.SagaId, i.ServiceName, i.MachineName, i.TimeoutState, i.Data, i.ExpiresUtc });

					Parallel.ForEach(timeoutParams, p =>
						{
							using (var connection = GetConnection())
							{
								connection.Execute("TimeoutData_Insert", p, commandType: CommandType.StoredProcedure);
							}
						});

					Parallel.ForEach(timeouts, timeoutData =>
					{
						using (var connection = GetConnection())
						{
							var timeoutDataParam = timeoutData.MessageDictionary.Select(i => new { timeoutData.TimeoutDataId, KeyName = i.Key, KeyValue = i.Value }).ToList();
							connection.Execute("INSERT INTO TimeoutDataValues (TimeoutDataId, KeyName, KeyValue) VALUES (@TimeoutDataId, @KeyName, @KeyValue)", timeoutDataParam, commandType: CommandType.Text);
						}
					});

				}
			}

			using (var connection = GetConnection())
			{
				connection.Execute("DELETE FROM SagaData WHERE Archive = 1;");
			}
		}
	}

	public interface ISagaInstrumentationProvider : IInstrumentationProvider
	{
		
	}
}
