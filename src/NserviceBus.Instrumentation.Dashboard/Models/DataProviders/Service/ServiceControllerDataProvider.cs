using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace NServiceBus.Instrumentation.Dashboard.Models.DataProviders.Service
{
	public class ServiceControllerDataProvider : IServiceControllerDataProvider
	{
		private readonly string connectionString;

		public ServiceControllerDataProvider(ConnectionStringSettings connectionStringSettings)
		{
			connectionString = connectionStringSettings.ConnectionString;
		}

		private DbConnection GetConnection()
		{
			var connection = new SqlConnection(connectionString);
			connection.Open();

			return connection;
		}

		public DetailDataModel GetDetailModel(string machineName, string serviceName)
		{
			using (var cn = GetConnection())
			{
				var model = new DetailDataModel
					{
						MachineName = machineName,
						ServiceName = serviceName
					};

				using(var multi = cn.QueryMultiple("Web_ServiceController_Detail", new { machineName, serviceName }, commandType:CommandType.StoredProcedure))
				{
					model.Sagas = new List<DetailDataModel.SagaClass>();

					var timeouts = new List<DetailDataModel.TimeoutClass>();
					
					multi.Read<DetailDataModel.SagaClass, KeyValuePair<string, string>, DetailDataModel.SagaClass>((saga, keyvalue) =>
						{
							var existingSaga = model.Sagas.FirstOrDefault(s => s.SagaDataId == saga.SagaDataId);

							if (existingSaga == null)
							{
								existingSaga = saga;
								model.Sagas.Add(existingSaga);
							}

							existingSaga.Values.Add(keyvalue.Key, keyvalue.Value); 
							
							return existingSaga;

						}, "SagaDataId").ToList();

					multi.Read<DetailDataModel.TimeoutClass, KeyValuePair<string, string>, DetailDataModel.TimeoutClass>((timeout, keyvalue) =>
						{
							var existingTimeout = timeouts.FirstOrDefault(t => t.TimeoutDataId == timeout.TimeoutDataId);

							if (existingTimeout == null)
							{
								existingTimeout = timeout;
								timeouts.Add(existingTimeout);
							}

							existingTimeout.Values.Add(keyvalue.Key, keyvalue.Value);

							return existingTimeout;
						}, "TimeoutDataId").ToList();

					timeouts.ForEach(t =>
						{
							var saga = model.Sagas.FirstOrDefault(s => s.SagaId == t.SagaId);

							if (saga != null)
							{
								saga.Timeouts.Add(t);
							}
						});

					model.Errors = multi.Read<DetailDataModel.Error>().ToList();
				}				

				return model;
			}
		}
	}

	public interface IServiceControllerDataProvider : IDataProvider
	{
		DetailDataModel GetDetailModel(string machineName, string serviceName);
	}
}