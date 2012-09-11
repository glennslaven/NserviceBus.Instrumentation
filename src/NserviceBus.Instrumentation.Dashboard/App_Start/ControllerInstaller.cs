using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NserviceBus.Instrumentation.Dashboard.Models.DataProviders;

namespace NserviceBus.Instrumentation.Dashboard.App_Start
{
	public class ControllerInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<HttpContextBase>().LifeStyle.PerWebRequest.UsingFactoryMethod(() => new HttpContextWrapper(HttpContext.Current)));			
			container.Register(Component.For<ConnectionStringSettings>().LifeStyle.PerWebRequest.Instance(ConfigurationManager.ConnectionStrings["Instrumentation"]));

			container.Register(AllTypes.FromThisAssembly()
								.Where(type => typeof(IDataProvider).IsAssignableFrom(type))								
								.WithService.FirstInterface()
								.LifestylePerWebRequest());

			container.Register(AllTypes.FromThisAssembly()
								.BasedOn<IController>()
								.If(t => typeof(Controller).IsAssignableFrom(t))
								.LifestylePerWebRequest());
		}
	}
}