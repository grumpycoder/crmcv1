
(function () {
    'use strict';
    var serviceId = 'spinnerInterceptor';
    angular.module('app').factory(serviceId, ['$q', 'appSpinner', spinnerInterceptor]);

    function spinnerInterceptor($q, appSpinner) {
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
                    alert(response.data.Message);
                    return $q.reject(hide(response));
                }
                if (response && response.data && response.data.Error === false && response.data.Message) {
                    alert(response.data.Message);
                }

                return hide(response);
            },
            'responseError': function (response) {
                if (!response)
                    return $q.reject(hide(response));

                if (response.data && response.data.Error &&
                    response.data.Error === true && response.data.Message) {
                    alert(response.data.Message);
                } else {
                    alert('Sorry, there was an error.');
                }

                return $q.reject(hide(response));
            }
        };
    }
})();
