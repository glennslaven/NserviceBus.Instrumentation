using Castle.MicroKernel.Registration;

namespace NServiceBus.Instrumentation.Providers.Error
{
	public class WindsorInstaller : IWindsorInstaller
	{
		public void Install(Castle.Windsor.IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
		{
			container.Register(Component.For<IErrorInstrumentationProvider>().LifestylePerThread().ImplementedBy<ErrorInstrumentationProvider>().Named("ErrorInstrumentationProvider"));
		}
	}
}
