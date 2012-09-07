using Castle.MicroKernel.Registration;

namespace NserviceBus.Instrumentation.Providers.Saga
{
	public class WindsorInstaller : IWindsorInstaller
	{
		public void Install(Castle.Windsor.IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
		{
			container.Register(Component.For<ISagaInstrumentationProvider>().LifestylePerThread().ImplementedBy<SagaInstrumentationProvider>().Named("SagaInstrumentationProvider"));
			container.Register(Component.For<IRavenSagaProvider>().LifestylePerThread().ImplementedBy<RavenSagaProvider>());
		}
	}
}
