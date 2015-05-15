(function () {
    'use strict';

    var factoryId = 'authenticator';

    angular.module('app').factory(factoryId, authenticator);

    authenticator.$inject = ['$q', '$location', 'currentUser', 'datacontext', 'localStorageService'];

    function authenticator($q, $location, currentUser, datacontext, localStorageService) {

        var authData = {
            isAuth: false,
            userName: '',
            userRetreived: false,
            firstName: '',
            lastName: '',
            email: '',
            roles: []
        };

        var service = {
            authData: authData,
            login: login,
            logOut: logOut,
            fillData: fillData
        };

        return service;

        function login(loginData) {
            return datacontext.login(loginData)
                .then(function (result) {
                    localStorageService.set('authorizationData',
                    { token: result.access_token, userName: loginData.userName });

                    authData.isAuth = true;
                    authData.userName = loginData.userName;
                    authData.userRetreived = false;
                    return result;
                }, function (error) {
                    return $q.reject(error);
                });
        }

        function logOut() {
            datacontext.logout().then(function () {
                clearAuthStorage();
                $location.path('/').replace();
            }, function () {
                clearAuthStorage();
                $location.path('/').replace();
            });
        }

        function clearAuthStorage() {
            localStorageService.remove('authorizationData');
            authData.isAuth = false;
            authData.userName = '';
            authData.userRetreived = false;
            authData.firstName = '';
            authData.lastName = '';
            authData.email = '';
            authData.roles.slice(0, authData.roles.length);

        }

        function fillData() {
            var data = localStorageService.get('authorizationData');
            if (data) {
                authData.isAuth = true;
                authData.userName = data.userName;
                if (!authData.userRetreived) {
                    return datacontext.getUserInfo().then(function (result) {
                        authData.userRetreived = true;
                        var userData = result.data;
                        authData.email = userData.email;
                        authData.roles = userData.roles;
                        authData.firstName = userData.firstName;
                        authData.lastName = userData.lastName;
                    });
                }
            }

            return $q.when(true);
        }
    }
})();