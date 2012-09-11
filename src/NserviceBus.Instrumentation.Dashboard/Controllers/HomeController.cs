using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AutoMapper;
using NserviceBus.Instrumentation.Dashboard.Models.DataProviders.Home;
using NserviceBus.Instrumentation.Dashboard.Models.Home;

namespace NserviceBus.Instrumentation.Dashboard.Controllers
{
    public class HomeController : Controller
    {
		static HomeController()
		{
			Mapper.CreateMap<IndexDataModel, IndexViewModel>();
			Mapper.CreateMap<IndexDataModel.Machine, IndexViewModel.Machine>();
			Mapper.CreateMap<IndexDataModel.Machine.Service, IndexViewModel.Machine.Service>();
		}

	    public IHomeControllerDataProvider DataProvider { get; set; }

        public ActionResult Index()
        {
			var dataModel = DataProvider.GetIndexModel();
			var viewModel = Mapper.Map<IndexViewModel>(dataModel);

			var serializer = new JavaScriptSerializer();
			serializer.MaxJsonLength = Int32.MaxValue;

			viewModel.JsonModel = new MvcHtmlString(serializer.Serialize(viewModel));

            return View(viewModel);
        }

    }
}
