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

		static string GetEndpointName(string path)
		{		
			var assemblyFiles = new DirectoryInfo(path).GetFiles("*.dll", SearchOption.AllDirectories)
				.Union(new DirectoryInfo(path).GetFiles("*.exe", SearchOption.AllDirectories));

			assemblyFiles = assemblyFiles.Where(f => !defaultAssemblyExclusions.Any(exclusion => f.Name.ToLower().StartsWith(exclusion)));			

			var results = new List<Assembly>();
			foreach (var assemblyFile in assemblyFiles)
			{
				try
				{
					var pd = new ProxyDomain();
					var assembly = pd.GetAssembly(assemblyFile.FullName);
					assembly.GetTypes();
					results.Add(assembly);
				}
				catch(Exception e)
				{
					
				}
			}


			var endPointType = ScanAssembliesForEndpoints(results).FirstOrDefault();

			if (endPointType == null)
			{
				throw new Exception(string.Format("No implementation of IConfigureThisEndpoint found in {0}", path));
			}

			//Stolen from https://github.com/NServiceBus/NServiceBus/blob/master/src/hosting/NServiceBus.Hosting.Windows/Program.cs
			var endpointConfiguration = Activator.CreateInstance(endPointType);
			var endpointName = endpointConfiguration.GetType().Namespace;

			var arr = endpointConfiguration.GetType().GetCustomAttributes(typeof(EndpointNameAttribute), false);
			if (arr.Length == 1)
				endpointName = (arr[0] as EndpointNameAttribute).Name;

			if (endpointConfiguration is INameThisEndpoint)
				endpointName = (endpointConfiguration as INameThisEndpoint).GetName();

			return endpointName;
		}

		//Stolen from https://github.com/NServiceBus/NServiceBus/blob/master/src/hosting/NServiceBus.Hosting.Windows/Program.cs
		static IEnumerable<Type> ScanAssembliesForEndpoints(List<Assembly> assemblyScannerResults)
		{
			var scannableAssemblies = assemblyScannerResults;
			return scannableAssemblies.SelectMany(assembly => assembly.GetTypes().Where(
				t => typeof(IConfigureThisEndpoint).IsAssignableFrom(t)
				     && t != typeof(IConfigureThisEndpoint)
				     && !t.IsAbstract));
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

			var managementObjectCollection = searcher.Get();
			foreach (ManagementObject mo in managementObjectCollection)
			{
				var path = mo.GetPropertyValue("PathName").ToString();
				if (path.ToLower().Contains("nservicebus.host.exe"))
				{
					var pathArray = path.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
					var fileInfo = new FileInfo(pathArray[0]);										
					var endpointName = GetEndpointName(fileInfo.Directory.FullName);

					//HACK: Currently this assumes that your dll containing the IConfigureThisEndpoint implementation is named 
					// the same as the namespace for that class
					var configPath = Path.Combine(fileInfo.DirectoryName, string.Format("{0}.dll.config", endpointName));
					if (!File.Exists(configPath))
					{
						throw new Exception(string.Format("{0} does not exist", configPath));
					}

					var serviceInfo = new ServiceInfo
					{
						Name = endpointName,
						FolderPath = fileInfo.DirectoryName,
						ConfigPath = configPath
					};

					var map = new ExeConfigurationFileMap {ExeConfigFilename = serviceInfo.ConfigPath};
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

			return nservicebusServices;
		}

		static readonly IEnumerable<string> defaultAssemblyExclusions = new[] { "system.", "nhibernate.", "log4net.", "raven.server.",
            "raven.client.", "raven.database", "raven.munin.", "raven.storage.", "raven.abstractions.", "lucene.net.", "bouncycastle.crypto",
            "esent.interop.", "asyncctplibrary.", "nservicebus.", "castle.", "dapper.", "mysql."};
		
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
