using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace NserviceBus.Instrumentation.Dashboard.Models.DataProviders.Service
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
				var model = new DetailDataModel();

				using(var multi = cn.QueryMultiple("Web_ServiceController_Detail", new { machineName, serviceName }, commandType:CommandType.StoredProcedure))
				{
					model.Sagas = new List<DetailDataModel.SagaClass>();

					var results = multi.Read<DetailDataModel.SagaClass, DetailDataModel.KeyValueClass, DetailDataModel.SagaClass>((saga, keyvalue) =>
						{
							var existingSaga = model.Sagas.FirstOrDefault(s => s.SagaDataId == saga.SagaDataId);

							if (existingSaga == null)
							{
								existingSaga = saga;
								model.Sagas.Add(existingSaga);
							}

							existingSaga.Values.Add(keyvalue); 
							
							return existingSaga;

						}, "SagaDataId").ToList();


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