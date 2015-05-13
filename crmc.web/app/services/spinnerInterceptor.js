
(function () {
    'use strict';
    var serviceId = 'spinnerInterceptor';
    angular.module('app').factory(serviceId, ['$q', 'appSpinner', 'common', spinnerInterceptor]);

    function spinnerInterceptor($q, appSpinner, common) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(serviceId);
        var logError = getLogFn(serviceId, 'error');
        var logSuccess = getLogFn(serviceId, 'success');

        var numRequests = 0;
        var hide = function (r) {
            if (!--numRequests) {
                appSpinner.hideSpinner();
            }
            return r;
        }

        return {
            'request': function (config) {
                numRequests++;
                appSpinner.showSpinner();
                return config;
            },
            'response': function (response) {
                if (response && response.data && response.data.ERROR && response.data.ERROR === true && response.data.Message) {
                    log(response.data.Message, null, false);
                    return $q.reject(hide(response));
                }
                if (response && response.data && response.data.Error === false && response.data.Message) {
                    log(response.data.Message, null, false);
                }

                return hide(response);
            },
            'responseError': function (response) {
                if (!response)
                    return $q.reject(hide(response));

                if (response.data && response.data.Error &&
                    response.data.Error === true && response.data.Message) {
                    log(response.data.Message);
                } else {
                    log('Sorry, there was an error.', response, false);
                }

                return $q.reject(hide(response));
            }
        };
    }
})();
