using System.Web.Mvc;

namespace NServiceBus.Instrumentation.Dashboard
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}