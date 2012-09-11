using System.Web.Mvc;
using AutoMapper;
using NserviceBus.Instrumentation.Dashboard.Models.DataProviders.Service;
using NserviceBus.Instrumentation.Dashboard.Models.Service;

namespace NserviceBus.Instrumentation.Dashboard.Controllers
{
    public class ServiceController : Controller
    {
	    public IServiceControllerDataProvider DataProvider { get; set; }

		static ServiceController()
		{
			Mapper.CreateMap<DetailDataModel, DetailViewModel>();
		}

        public ActionResult Detail(string id, string service)
        {
			var dataModel = DataProvider.GetDetailModel(id, service);
			var viewModel = Mapper.Map<DetailViewModel>(dataModel);

            return View(viewModel);
        }

    }
}
