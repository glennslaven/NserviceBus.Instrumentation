using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Messaging;
using System.Text;
using Dapper;
using NServiceBus.Instrumentation.Interfaces;
using NServiceBus.Utils;
using log4net;

namespace NServiceBus.Instrumentation.Providers.Error
{
	public class ErrorInstrumentationProvider : IErrorInstrumentationProvider
    {
		List<IServiceInfo> Services { get; set; }
		public ILog Logger { get; set; }
		public ConnectionStringSettings ConnectionStringSettings { get; set; }

		string MachineName { get; set; }
		string PrivateQueuePrefix {get; set;}

		private DbConnection GetConnection()
		{
			var connection = new SqlConnection(ConnectionStringSettings.ConnectionString);
			connection.Open();

			return connection;
		}

		public void Instrument()
		{
			foreach (var serviceInfo in Services)
			{
				var address = new Address(serviceInfo.ErrorQueue, MachineName);
				var queue = new MessageQueue(MsmqUtilities.GetFullPath(address));
				var filter = new MessagePropertyFilter();
				filter.SetAll();
				queue.MessageReadPropertyFilter = filter;

				var messages = queue.GetAllMessages();

				foreach (var message in messages)
				{
					var transportMessage = MsmqUtilities.Convert(message);

					if (transportMessage.Headers.ContainsKey(Faults.FaultsHeaderKeys.FailedQ))
					{
						var value = Address.Parse(transportMessage.Headers[Faults.FaultsHeaderKeys.FailedQ]);
						if (serviceInfo.Name.Equals(value.Queue, StringComparison.InvariantCultureIgnoreCase))
						{
							var messageText = Encoding.UTF8.GetString(transportMessage.Body);

							using (var con = GetConnection())
							{
								const string sql = "INSERT INTO Error (ErrorId, ServiceName, MachineName, Message, ErrorMessage, Stacktrace, MessageSentTimeUtc) VALUES(@errorId, @serviceName, @machineName, @message, @errorMessage, @stacktrace, @messageSentTimeUtc)";
								var errorMessage = transportMessage.Headers["NServiceBus.ExceptionInfo.Message"];
								var stackTrace = transportMessage.Headers["NServiceBus.ExceptionInfo.StackTrace"];
								var messageSentTimeUtc = transportMessage.Headers["NServiceBus.TimeOfFailure"].ToUtcDateTime();

								con.Execute(sql, new { ErrorId = Guid.NewGuid(), serviceName = serviceInfo.Name, MachineName, message = messageText, errorMessage, stackTrace, messageSentTimeUtc });
							}
						}
					}
				}
			}
		}

		public void Setup(IEnumerable<IServiceInfo> services)
		{
			Services = services.Where(s => !string.IsNullOrWhiteSpace(s.ErrorQueue)).ToList();
			MachineName = Environment.MachineName;
			PrivateQueuePrefix = string.Format(@"FormatName:DIRECT=OS:{0}\", MachineName);
		}
    }

	public interface IErrorInstrumentationProvider : IInstrumentationProvider
	{

	}
}
