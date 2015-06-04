
(function () {
    'use strict';

    var controllerId = 'LoginCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$http', 'common', 'config', 'currentUser', 'loginRedirect', LoginCtrl]);

    function LoginCtrl($scope, $http, common, config, currentUser, loginRedirect) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var logError = getLogFn(controllerId, 'error');
        var logSuccess = getLogFn(controllerId, 'success');

        vm.activate = activate;
        vm.title = 'Login';

        vm.login = login;
        vm.logout = logout;
        vm.message = '';
        vm.user = currentUser.profile; 

        $scope.userData = {
            username: '',
            email: '',
            password: '',
            confirmPassword: ''
        };

        function activate() {
        }

        //#region Internal Methods        

        function login() {
//            var url = config.serverPath + '/token';
            var url = common.serverUri() + '/token';
            
            var data = 'username=' + $scope.userData.username +
                '&password=' + $scope.userData.password +
                '&grant_type=password';
            var configuration = {
                headers: { 'Content-Type': 'application/x-www-form-urlencode' }
            };

            return $http.post(url, data, configuration)
                .then(processToken($scope.userData.username))
                .then(getUserInfo($scope.userData.username))
                .then(loginRedirect.redirectPreLogin)
            .catch(function (response) {
                log('login response', response, false);
                vm.message = response.data.error_description; 
            });
        }

        function logout() {
            currentUser.profile.username = '';
            currentUser.profile.token = '';
            currentUser.remove();
        }

        function getUserInfo(username) {
            //var url = common.serverUri() + '/api/accounts/localuserinfo/' + currentUser.profile.username + '/';
            var url = common.serverUri() + '/api/accounts/localuserinfo/' + username + '/';
            $http.get(url).then(function (response) {
                currentUser.profile.roles = response.data.roles;
                currentUser.save();
            })
        }

        function processToken(username) {
            
            return function (response) {
                currentUser.profile.username = username;
                currentUser.profile.token = response.data.access_token;
                currentUser.save();
                return username;
            };
        };
        //#endregion
    }
})();
