using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using Dapper;

namespace NServiceBus.Instrumentation.Dashboard.Models.DataProviders.Home
{
	public class HomeControllerDataProvider : IHomeControllerDataProvider
	{
		private readonly string connectionString;

		public HomeControllerDataProvider(ConnectionStringSettings connectionStringSettings)
		{
			connectionString = connectionStringSettings.ConnectionString;
		}

		private DbConnection GetConnection()
		{
			var connection = new SqlConnection(connectionString);
			connection.Open();

			return connection;
		}

		public IndexDataModel GetIndexModel()
		{
			using (var cn = GetConnection())
			{
				var data = cn.Query("SELECT DISTINCT servicename, machinename FROM SagaData UNION SELECT DISTINCT servicename, machinename FROM Error  ORDER BY machinename, servicename");

				var model = new IndexDataModel();

				model.Machines = new List<IndexDataModel.Machine>();

				var currentMachine = string.Empty;
				var machine = new IndexDataModel.Machine();
				foreach (var i in data)
				{
					if (i.machinename.ToString().ToLower() != currentMachine)
					{
						currentMachine = i.machinename.ToString().ToLower();

						machine = new IndexDataModel.Machine
							{
								Name = i.machinename,
								Services = new List<IndexDataModel.Machine.Service>()
							};
						model.Machines.Add(machine);

					}

					machine.Services.Add(new IndexDataModel.Machine.Service { Name = i.servicename });
				}

				return model;
			}
		}
	}

	public interface IHomeControllerDataProvider : IDataProvider
	{
		IndexDataModel GetIndexModel();
	}
}