using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using NServiceBus;

namespace NServiceBus.Instrumentation.Agent
{
	public class ServiceLoader
	{
		private class ServiceInfo
		{
			public string Name { get; set; }
			public string FolderPath { get; set; }
			public string ConfigPath { get; set; }
		}

		static void EnumServices(string host, string username, string password)
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

					var fileInfos = fileInfo.Directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly).ToList();
					fileInfos.ForEach(a =>
					{
						var assembly = pd.GetAssembly(a.FullName);
						try
						{
							var endpointConfig = assembly.GetTypes().FirstOrDefault(t => typeof(IConfigureThisEndpoint).IsAssignableFrom(t));
							if (endpointConfig != null)
							{
								var configPath = Path.Combine(fileInfo.DirectoryName, string.Format("{0}.config", a.FullName));

								var serviceInfo = new ServiceInfo
								{
									Name = endpointConfig.Namespace,
									FolderPath = fileInfo.DirectoryName,
									ConfigPath = File.Exists(configPath) ? configPath : null
								};
								nservicebusServices.Add(serviceInfo);
							}
						}
						catch (Exception) { }
					});



				}
			}
		}

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
