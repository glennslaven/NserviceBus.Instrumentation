using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Timers;
using NserviceBus.Instrumentation.Interfaces;
using log4net;

namespace NserviceBus.Instrumentation.Agent
{
	internal class AgentService : IAgentService
	{
		private const string RetriesSuffix = ".retries";
		private const string PrivateQueuePrefix = "private$\\";
		private readonly Timer timer;

		private readonly List<NserviceBusService> nserviceBusServices = new List<NserviceBusService>();
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
			foreach (InstrumentationConfig.Provider providerConfig in Config.Providers)
			{
				var provider = ProviderFactory.Construct(providerConfig.Name);
				provider.Setup(nserviceBusServices.Select(s => s.Name));
				instrumentationProviders.Add(provider);
			}

			var queues = MessageQueue.GetPrivateQueuesByMachine(".").ToList();

			if (Config.Services.Autoload)
			{
				var services = queues
					.Where(queue => queue.QueueName.EndsWith(RetriesSuffix))
					.Select(queue => queue.QueueName.Replace(RetriesSuffix, string.Empty))
					.Where(queueName => queues.Exists(q => q.QueueName == queueName))
					.Select(queueName => queueName.Replace(PrivateQueuePrefix, string.Empty));

				foreach (var serviceName in services)
				{
					if (Config.Services.Whitelist.Count > 0 && Config.Services.Whitelist.Contains(serviceName) && !Config.Services.Blacklist.Contains(serviceName))
					{
						nserviceBusServices.Add(new NserviceBusService
							{
								Name = serviceName
							});

						Logger.InfoFormat("Service found {0}", serviceName);
					}
				}
			}

			foreach (InstrumentationConfig.Service service in Config.Services.ExplicitList)
			{
				if (!queues.Exists(q => q.QueueName == PrivateQueuePrefix + service.Name))
				{
					throw new ConfigurationErrorsException(string.Format("No queue exists for service names {0}", service.Name));
				}

				nserviceBusServices.Add(new NserviceBusService
				{
					Name = service.Name
				});

				Logger.InfoFormat("Service found {0}", service.Name);
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
