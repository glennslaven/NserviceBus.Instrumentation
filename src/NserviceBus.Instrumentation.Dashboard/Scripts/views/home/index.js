/// <reference path="../../knockout-2.0.0.debug.js" />


if (!NserviceBus) var NserviceBus = {};
if (!NserviceBus.Instrumentation) NserviceBus.Instrumentation = {};
if (!NserviceBus.Instrumentation.Dashboard) NserviceBus.Instrumentation.Dashboard = {};

NserviceBus.Instrumentation.Dashboard.Index = (function () {

    return {
        Bind: function (jsonModel) {
            

            var mapping = {
                'Machines': {
                    create: function (options) {
                        
                        var data = options.data;
                        data.ShowServices = ko.observable(false);
                        data.toggleServices = function (element, event) {
                            element.ShowServices(!element.ShowServices());
                        };
                        
                        return data;
                    }
                }
            };

            var viewModel = ko.mapping.fromJS(jsonModel, mapping);

            ko.applyBindings(viewModel);
        }
    };
})();