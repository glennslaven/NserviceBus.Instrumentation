using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;
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
			Mapper.CreateMap<DetailDataModel.SagaClass, DetailViewModel.SagaClass>();
			Mapper.CreateMap<DetailDataModel.KeyValueClass, DetailViewModel.KeyValueClass>();
		}

        public ActionResult Detail(string id, string service)
        {
			var dataModel = DataProvider.GetDetailModel(id, service);
			var viewModel = Mapper.Map<DetailViewModel>(dataModel);

			var serializer = new JavaScriptSerializer();
			serializer.MaxJsonLength = Int32.MaxValue;

			viewModel.JsonModel = new MvcHtmlString(serializer.Serialize(viewModel));

            return View(viewModel);
        }

    }
}
