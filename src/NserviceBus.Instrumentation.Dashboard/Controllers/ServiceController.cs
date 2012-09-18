using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AutoMapper;
using NServiceBus.Instrumentation.Dashboard.Models.DataProviders.Service;
using NServiceBus.Instrumentation.Dashboard.Models.Service;

namespace NServiceBus.Instrumentation.Dashboard.Controllers
{
    public class ServiceController : Controller
    {
	    public IServiceControllerDataProvider DataProvider { get; set; }

		static ServiceController()
		{
			Mapper.CreateMap<DetailDataModel, DetailViewModel>();
			Mapper.CreateMap<DetailDataModel.SagaClass, DetailViewModel.SagaClass>();
			Mapper.CreateMap<DetailDataModel.TimeoutClass, DetailViewModel.TimeoutClass>();
		}

        public ActionResult Detail(string id, string service)
        {
			var dataModel = DataProvider.GetDetailModel(id, service);
			var viewModel = Mapper.Map<DetailViewModel>(dataModel);

			viewModel.Sagas.ForEach(s => 
				s.Timeouts.ForEach(t =>
					t.Values.Add("Expires", t.ExpiresUtc.ToLocalTime().ToString("dd MMM yyyy HH:mm:ss"))));

			var serializer = new JavaScriptSerializer();
			serializer.MaxJsonLength = Int32.MaxValue;

			viewModel.JsonModel = new MvcHtmlString(serializer.Serialize(viewModel));

            return View(viewModel);
        }

    }
}
