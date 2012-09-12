if (!NserviceBus) var NserviceBus = {};
if (!NserviceBus.Instrumentation) NserviceBus.Instrumentation = {};
if (!NserviceBus.Instrumentation.Dashboard) NserviceBus.Instrumentation.Dashboard = {};
if (!NserviceBus.Instrumentation.Dashboard.Service) NserviceBus.Instrumentation.Dashboard.Service = {};

NserviceBus.Instrumentation.Dashboard.Service.Detail = (function () {

    return {
        Bind: function (jsonModel) {
           var sagasMapping = {
               'Sagas': {
                   create: function (options) {
                        
                       var data = ko.mapping.fromJS(options.data, timoutsMapping);
                       data.ShowValues = ko.observable(false);
                       data.FilteredOut = ko.observable(false);
                       data.Values = ko.observableDictionary(options.data.Values);
                       return data;
                   }
               },
           };
            
           var timoutsMapping = {
               'Timeouts': {
                   create: function (options) {
                       var data = ko.mapping.fromJS(options.data);
                       data.Values = ko.observableDictionary(options.data.Values);
                       return data;
                   }
               }
           };

           function viewModel(json) {
                var self = this;
                
                self.Sagas = ko.mapping.fromJS(json, sagasMapping).Sagas;
                self.SelectedManually = ko.observable();               
                self.FilterText = ko.observable();
                self.FilterKeyName = ko.observable();                                

                self.DisplaySagas = ko.computed(function () {
                    var filter = self.FilterText();
                    if (!filter) {
                        return this.Sagas();
                    } else {
                        return ko.utils.arrayFilter(this.Sagas(), function (item) {
                            return item.Values.get(self.FilterKeyName())().startsWith(filter.toLowerCase(), true);
                        });
                    }
                }, self);

                self.Selected = ko.computed(function () {
                    if (self.SelectedManually() && ko.utils.arrayIndexOf(self.DisplaySagas(), self.SelectedManually()) > -1) {
                        return self.SelectedManually();
                    } else {
                        return self.DisplaySagas()[0];
                    }                    
                });

                self.toggleValues = function (saga) {

                    var currentState = saga.ShowValues();

                    for (var i in self.Sagas()) {
                        self.Sagas()[i].ShowValues(false);
                    }

                    saga.ShowValues(!currentState);
                };
               
                self.clearFilter = function () {
                    self.FilterText('');
                };
            };

           ko.applyBindings(new viewModel(jsonModel));
        }
    };
})();