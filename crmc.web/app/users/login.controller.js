
(function () {
    'use strict';

    var controllerId = 'LoginCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$http', 'config', 'currentUser', 'loginRedirect', LoginCtrl]);

    function LoginCtrl($scope, $http, config, currentUser, loginRedirect) {
        var vm = this;

        vm.activate = activate;
        vm.title = 'Login';

        vm.login = login;
        vm.logout = logout;

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

        function logout() {
            currentUser.profile.username = '';
            currentUser.profile.token = '';
            currentUser.remove();
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
