using Castle.MicroKernel.Registration;
using log4net;
using System.Configuration;

namespace NServiceBus.Instrumentation.Agent
{
	public class WindsorInstaller : IWindsorInstaller
	{
		public void Install(Castle.Windsor.IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
		{
			container.Register(Component.For<IAgentService>().LifestylePerThread().ImplementedBy<AgentService>());
			container.Register(Component.For<ILog>().LifestylePerThread().UsingFactoryMethod((kernel, componentModel, creationContext) => LogManager.GetLogger(creationContext.Handler.ComponentModel.Implementation)));
			container.Register(Component.For<InstrumentationConfig>().LifestyleSingleton().Instance(InstrumentationConfig.Config));
			container.Register(Component.For<IProviderFactory>().LifestyleSingleton().UsingFactoryMethod(kernel => new ProviderFactory { Kernel = kernel}));
			container.Register(Component.For<ConnectionStringSettings>().LifestyleSingleton().Instance(ConfigurationManager.ConnectionStrings["NSBInstrumentation"]));	
		}
	}
}
