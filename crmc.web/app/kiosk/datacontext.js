
(function () {
    'use strict';
    var serviceId = 'datacontext';
    angular.module('app').factory(serviceId, ['$http', 'common', 'config', datacontext]);

    function datacontext($http, common, config) {
        var $q = common.$q;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(serviceId);
        var logError = getLogFn(serviceId, 'error');
        var logSuccess = getLogFn(serviceId, 'success');

        var urls = config.urls;

        var service = {
            getBlackList: getBlackList,
            getPersonByName: getPersonByName,
            getPersons: getPersons,
            getPersonCount: getPersonCount,
            getWallConfig: getWallConfig,
            saveConfigValues: saveConfigValues,
            savePerson: savePerson
        };

        return service;

        function getBlackList() {
            return $http.get(urls.getBlackList)
                .then(function (resp) {
                    return resp.data;
                })
                .catch(function (resp) {
                    return resp.status;
                });
        }

        function getPersonCount(filter) {
            var fn = filter.split(' ')[0] || '';
            var ln = filter.split(' ')[1] || fn;
            if (fn === ln) { fn = '' };

            return $http.get(urls.getPerson + '?firstname=' + fn + '&lastname=' + ln + '&listsize=' + 100000000);
        }

        function getPersons(page, size, filter) {
            var take = size || 20;
            var skip = page ? (page - 1) * size : 0;

            var fn = filter.split(' ')[0] || '';
            var ln = filter.split(' ')[1] || fn;
            if (fn === ln) { fn = '' };

            return $http.get(urls.getPerson + '?firstname=' + fn + '&lastname=' + ln + '&skipCount=' + skip + '&listSize=' + take);

        }

        function getPersonByName(person) {
            $http({
                method: 'GET',
                url: 'api/person'
            }).then(sendResponse);

            function sendResponse(response) {
                return response.data;
            }
        }

        function getWallConfig() {
            return $http.get(urls.getConfig).success(function (resp) {
//                log('retrieved data', resp, false);
            });
        }

        function saveConfigValues(config) {
            return $http.post(urls.saveConfig, config).success(function () {
//                log('saved configuration values', config, false);
            });
        }

        function savePerson(person) {
            return $http.post(urls.postPerson, JSON.stringify(person))
                .success(function (resp) {
                    log('save successful', resp, false);
                    return resp.status;
                })
                .catch(function (resp) {
                    return resp.status;
                });
        }

    }
})();
