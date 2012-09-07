using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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

					using (var connection = GetConnection())
					{
						connection.Execute("SagaData_Insert", data, commandType:CommandType.StoredProcedure);
					}
				}
			}
		}
	}

	public interface ISagaInstrumentationProvider : IInstrumentationProvider
	{
		
	}
}
