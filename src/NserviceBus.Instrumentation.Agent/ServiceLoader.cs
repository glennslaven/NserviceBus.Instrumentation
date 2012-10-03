using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using NServiceBus.Config;
using NServiceBus.Instrumentation.Interfaces;

namespace NServiceBus.Instrumentation.Agent
{
	public static class ServiceLoader
	{
		[DebuggerDisplay("{Name}")]
		public class ServiceInfo : IServiceInfo
		{
			public string Name { get; set; }
			public string FolderPath { get; set; }
			public string ConfigPath { get; set; }
			public string ErrorQueue { get; set; }
			public string AuditQueue { get; set; }
		}

		public static List<ServiceInfo> EnumServices(string host, string username, string password)
		{
			const string ns = @"root\cimv2";
			const string query = "select * from Win32_Service";

			var options = new ConnectionOptions();
			if (!string.IsNullOrEmpty(username))
			{
				options.Username = username;
				options.Password = password;
			}

			var scope = new ManagementScope(string.Format(@"\\{0}\{1}", host, ns), options);
			scope.Connect();

			var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));

			var nservicebusServices = new List<ServiceInfo>();

			foreach (ManagementObject mo in searcher.Get())
			{
				var path = mo.GetPropertyValue("PathName").ToString();
				if (path.ToLower().Contains("nservicebus.host.exe"))
				{
					var pathArray = path.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
					var fileInfo = new FileInfo(pathArray[0]);

					var pd = new ProxyDomain();

					var fileInfos = fileInfo.Directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Where(f => !defaultAssemblyExclusions.Any(exclusion => f.Name.ToLower().StartsWith(exclusion))).ToList();
					fileInfos.ForEach(a =>
					{
						var assembly = pd.GetAssembly(a.FullName);
						try
						{
							var endpointConfig = assembly.GetTypes().Where(t => !defaultTypeExclusions.Any(exclusion => t.FullName.ToLower().StartsWith(exclusion))).FirstOrDefault(t => typeof(IConfigureThisEndpoint).IsAssignableFrom(t));
							if (endpointConfig != null)
							{
								var configPath = Path.Combine(fileInfo.DirectoryName, string.Format("{0}.config", a.FullName));

								var serviceInfo = new ServiceInfo
								{
									Name = endpointConfig.Namespace,
									FolderPath = fileInfo.DirectoryName,
									ConfigPath = File.Exists(configPath) ? configPath : null
								};


								var map = new ExeConfigurationFileMap() {ExeConfigFilename = serviceInfo.ConfigPath};

								var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
								
								
									
									var faultConfig = config.GetSection("MessageForwardingInCaseOfFaultConfig") as MessageForwardingInCaseOfFaultConfig;
								if (faultConfig != null)
								{
									serviceInfo.ErrorQueue = faultConfig.ErrorQueue;
								}

								var unicastBusConfig = config.GetSection("UnicastBusConfig") as UnicastBusConfig;
								if (unicastBusConfig != null)
								{
									serviceInfo.AuditQueue = unicastBusConfig.ForwardReceivedMessagesTo;
								}

								nservicebusServices.Add(serviceInfo);
							}
						}
						catch (Exception) { }
					});



				}
			}

			return nservicebusServices;
		}

		static readonly IEnumerable<string> defaultAssemblyExclusions = new[] { "system.", "nhibernate.", "log4net.", "raven.server.",
            "raven.client.", "raven.database", "raven.munin.", "raven.storage.", "raven.abstractions.", "lucene.net.", "bouncycastle.crypto",
            "esent.interop.", "asyncctplibrary.", "nservicebus.", "castle.", "dapper.", "mysql."};
		static readonly IEnumerable<string> defaultTypeExclusions = new[] { "raven.", "system.", "lucene.", "magnum." };

		class ProxyDomain : MarshalByRefObject
		{
			public Assembly GetAssembly(string AssemblyPath)
			{
				try
				{
					return Assembly.LoadFrom(AssemblyPath);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException("", ex);
				}
			}
		}
	}
}
