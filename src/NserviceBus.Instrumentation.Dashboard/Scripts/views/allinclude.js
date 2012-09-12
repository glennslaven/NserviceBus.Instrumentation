if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str, ignoreCase) {
        if (ignoreCase) {
            return this.toLowerCase().slice(0, str.length) == str.toLowerCase();
        } else {
             return this.slice(0, str.length) == str;
        }
    };
}


if (!NserviceBus) var NserviceBus = {};
if (!NserviceBus.Instrumentation) NserviceBus.Instrumentation = {};
if (!NserviceBus.Instrumentation.Dashboard) NserviceBus.Instrumentation.Dashboard = {};