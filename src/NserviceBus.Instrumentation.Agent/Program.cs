using Castle.Windsor;
using Castle.Windsor.Installer;
using NServiceBus.Instrumentation.Providers.Error;
using NServiceBus.Instrumentation.Providers.Saga;
using Topshelf;

namespace NServiceBus.Instrumentation.Agent
{
	public class Program
	{
		public static void Main()
		{
			var container = new WindsorContainer();
			container.Install(FromAssembly.This());
			
			container.Install(FromAssembly.Containing<ISagaInstrumentationProvider>());
			container.Install(FromAssembly.Containing<IErrorInstrumentationProvider>());

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
				x.SetDescription("NServiceBus Instrumentation Agent");
				x.SetDisplayName("NServiceBus Instrumentation Agent");
				x.SetServiceName("NSBIAgent");                       
			});                                                  
		}
	}
}
