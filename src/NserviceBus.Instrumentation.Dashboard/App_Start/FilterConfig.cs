using System.Web.Mvc;

namespace NserviceBus.Instrumentation.Dashboard
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}