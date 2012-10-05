if (!NserviceBus) var NserviceBus = {};
if (!NserviceBus.Instrumentation) NserviceBus.Instrumentation = {};
if (!NserviceBus.Instrumentation.Dashboard) NserviceBus.Instrumentation.Dashboard = {};
if (!NserviceBus.Instrumentation.Dashboard.Service) NserviceBus.Instrumentation.Dashboard.Service = {};

NserviceBus.Instrumentation.Dashboard.Service.Detail = (function () {

    return {
        Bind: function (jsonModel) {
           var baseMapping = {
               'Sagas': {
                   create: function (options) {
                        
                       var data = ko.mapping.fromJS(options.data, timoutsMapping);
                       data.ShowValues = ko.observable(false);
                       data.FilteredOut = ko.observable(false);
                       data.Values = ko.observableDictionary(options.data.Values);
                       return data;
                   }
               },
               'Errors': {
                   create: function (options) {
                       var data = ko.mapping.fromJS(options.data);
                       data.ShowDetails = ko.observable(false);
                       return data;
                   }
               }
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
                var mapping = ko.mapping.fromJS(json, baseMapping);
               
                self.Sagas = mapping.Sagas;
                self.Errors = mapping.Errors;
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
               
                self.FilterKeys = function () {
                    if (self.Sagas().length) {
                        return self.Sagas()[0].Values.items;
                    } else {
                        return {};
                    }
                };
            };

           ko.bindingHandlers.fadeVisible = {
               init: function (element, valueAccessor) {
                   // Initially set the element to be instantly visible/hidden depending on the value
                   var value = valueAccessor();
                   $(element).toggle(ko.utils.unwrapObservable(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
               },
               update: function (element, valueAccessor) {
                   // Whenever the value subsequently changes, slowly fade the element in or out
                   var value = valueAccessor();
                   ko.utils.unwrapObservable(value) ? $(element).fadeIn() : $(element).fadeOut();
               }
           };
            
           ko.bindingHandlers.slideVisible = {
               init: function (element, valueAccessor) {
                   var value = valueAccessor();
                   $(element).toggle(ko.utils.unwrapObservable(value));
               },
               update: function (element, valueAccessor) {
                   var value = valueAccessor();
                   ko.utils.unwrapObservable(value) ? $(element).slideDown() : $(element).slideUp();
               }
           };

           ko.applyBindings(new viewModel(jsonModel));
        }
    };
})();