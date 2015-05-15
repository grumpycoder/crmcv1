(function () {
    "use strict";

    function authService($http, config, currentUser) {

        var service = {
            login: login,
            logout: logout,
            getUserData: getUserData
        }

        return service;

        function login(loginData) {
            var url = config.serverPath + '/token';
            var data = 'username=' + $scope.userData.username +
                '&password=' + $scope.userData.password +
                '&grant_type=password';
            var configuration = {
                headers: { 'Content-Type': 'application/x-www-form-urlencode' }
            };

            $http.post(url, data, configuration)
                .then(processToken($scope.userData.username)).then(loginRedirect.redirectPreLogin)
            .catch(function (response) {

            });
        }

    }

    angular.module('common').factory('authService', ['$http', 'config', 'currentUser', authService]);
}());
