using System;
using System.Timers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using NserviceBus.Instrumentation.Providers.Saga;
using Topshelf;

namespace NserviceBus.Instrumentation.Agent
{
	public class TownCrier
	{
		readonly Timer _timer;
		public TownCrier()
		{
			_timer = new Timer(1000) { AutoReset = true };
			_timer.Elapsed += (sender, eventArgs) => Console.WriteLine("It is {0} an all is well", DateTime.Now);
		}
		public void Start() { _timer.Start(); }
		public void Stop() { _timer.Stop(); }
	}

	public class Program
	{
		public static void Main()
		{
			var container = new WindsorContainer();
			container.Install(FromAssembly.This());
			
			container.Install(FromAssembly.Containing<ISagaInstrumentationProvider>());

			HostFactory.Run(x =>                                
			{
				x.Service<IAgentService>(s =>                       
				{
					s.ConstructUsing(name => container.Resolve<IAgentService>());     
					s.WhenStarted(a => a.Start());              
					s.WhenStopped(a => a.Stop());
					s.WhenPaused(a => a.Pause());
					s.WhenContinued(a => a.Continue());
				});
				x.RunAsLocalSystem();
				x.DependsOnMsmq();
				x.DependsOnMsSql();
				x.SetDescription("NServiceBus Instrumentation Agent");
				x.SetDisplayName("NServiceBus Instrumentation Agent");
				x.SetServiceName("NSBIAgent");                       
			});                                                  
		}
	}
}
