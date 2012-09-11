/// <reference path="../../knockout-2.0.0.debug.js" />


if (!NserviceBus) var NserviceBus = {};
if (!NserviceBus.Instrumentation) NserviceBus.Instrumentation = {};
if (!NserviceBus.Instrumentation.Dashboard) NserviceBus.Instrumentation.Dashboard = {};
if (!NserviceBus.Instrumentation.Dashboard.Service) NserviceBus.Instrumentation.Dashboard.Service = {};

NserviceBus.Instrumentation.Dashboard.Service.Detail = (function () {

    return {
        Bind: function (jsonModel) {
            

           var mapping = {
                'Sagas': {
                    create: function (options) {
                        
                        var data = options.data;
                        data.ShowValues = ko.observable(false);
                        
                        return data;
                    }
                }
            };

            function viewModel() {
                var self = this;
                
                

                self.Sagas = ko.mapping.fromJS(jsonModel, mapping).Sagas;
                self.selected = ko.observable(self.Sagas()[0]);
                
                self.toggleValues = function (saga) {

                    var currentState = saga.ShowValues();

                    for (var i in self.Sagas()) {
                        self.Sagas()[i].ShowValues(false);
                    }

                    saga.ShowValues(!currentState);
                };
            };

            ko.applyBindings(new viewModel());
        }
    };
})();