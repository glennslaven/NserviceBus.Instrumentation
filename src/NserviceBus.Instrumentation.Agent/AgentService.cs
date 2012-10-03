using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Timers;
using NServiceBus.Instrumentation.Interfaces;
using log4net;

namespace NServiceBus.Instrumentation.Agent
{
	internal class AgentService : IAgentService
	{
		private const string RetriesSuffix = ".retries";
		private const string PrivateQueuePrefix = "private$\\";
		private readonly Timer timer;

		private readonly List<IServiceInfo> nserviceBusServices = new List<IServiceInfo>();
		private readonly List<IInstrumentationProvider> instrumentationProviders = new List<IInstrumentationProvider>();

		public ILog Logger { get; set; }
		public InstrumentationConfig Config { get; set; }
		public IProviderFactory ProviderFactory { get; set; }

		public AgentService()
		{
			timer = new Timer(3600000) { AutoReset = true };
			timer.Elapsed += timer_Elapsed;	
		}

		private void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			foreach (var provider in instrumentationProviders)
			{
				provider.Instrument();
			}
		}

		private void Setup()
		{
			var services = ServiceLoader.EnumServices(".", "", "");

			if (Config.Services.Autoload)
			{
				foreach (var service in services)
				{
					if (Config.Services.Whitelist.Count > 0 && Config.Services.Whitelist.Contains(service.Name) || !Config.Services.Blacklist.Contains(service.Name))
					{
						nserviceBusServices.Add(service);

						Logger.InfoFormat("Service found {0}", service.Name);
					}
				}
			}

			foreach (InstrumentationConfig.Service configService in Config.Services.ExplicitList)
			{
				var service = services.FirstOrDefault(s => s.Name == PrivateQueuePrefix + configService.Name);

				if (service == null)
				{
					throw new ConfigurationErrorsException(string.Format("No queue exists for service names {0}", configService.Name));
				}

				if (!nserviceBusServices.Contains(service))
				{
					nserviceBusServices.Add(service);
					Logger.InfoFormat("Service found {0}", service.Name);
				}
			}

			foreach (InstrumentationConfig.Provider providerConfig in Config.Providers)
			{
				var provider = ProviderFactory.Construct(providerConfig.Name);
				provider.Setup(nserviceBusServices);
				instrumentationProviders.Add(provider);
			}
		}

		public void Start() 
		{
			timer.Start();
			Setup();

			timer_Elapsed(timer, null);
		}
		
		public void Stop() 
		{
			timer.Stop();
		}

		public void Pause()
		{
			timer.Start();
		}
		
		public void Continue()
		{
			timer.Stop();
		}

		private class NserviceBusService
		{
			public string Name { get; set; }
		}
	}

	internal interface IAgentService
	{
		void Start();
		void Stop();
		void Pause();
		void Continue();
	}
}
