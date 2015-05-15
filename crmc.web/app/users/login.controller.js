
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
            username: 'abc@abc.com',
            email: 'abc@abc.com',
            password: '!1Password',
            confirmPassword: ''
        };

        function activate() {
        }

        //#region Internal Methods        

        function login() {
//            var url = config.serverPath + '/token';
            var url = common.serverUri() + '/token';
            log(common.serverUri());
            log(url);

            var data = 'username=' + $scope.userData.username +
                '&password=' + $scope.userData.password +
                '&grant_type=password';
            var configuration = {
                headers: { 'Content-Type': 'application/x-www-form-urlencode' }
            };

            $http.post(url, data, configuration)
                .then(processToken($scope.userData.username))
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

        function getUserInfo() {
            //            var url = config.serverPath + '/api/accounts/localuserinfo/' + currentUser.profile.username + '/'; 
            var url = common.serverUri + '/api/accounts/localuserinfo/' + currentUser.profile.username + '/';
            $http.get(url).then(function (response) {
                currentUser.profile.roles = response.data.roles;
                currentUser.save();
                log('userinfo response', response, false);
            })
        }

        function processToken(username) {
            return function (response) {
                currentUser.profile.username = username;
                currentUser.profile.token = response.data.access_token;
                getUserInfo();
                currentUser.save();
                return username;
            };
        };
        //#endregion
    }
})();
