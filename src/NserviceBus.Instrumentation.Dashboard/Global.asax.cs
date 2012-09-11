using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.Windsor.Installer;
using NserviceBus.Instrumentation.Dashboard.App_Start;

namespace NserviceBus.Instrumentation.Dashboard
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			var container = new Castle.Windsor.WindsorContainer();
			container.Install(FromAssembly.InThisApplication());

			var controllerFactory = new WindsorControllerFactory(container.Kernel);
			ControllerBuilder.Current.SetControllerFactory(controllerFactory);

			AreaRegistration.RegisterAllAreas();

			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}
	}
}