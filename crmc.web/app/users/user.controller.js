
(function () {
    'use strict';

    var controllerId = 'UserCtrl';
    angular.module('app').controller(controllerId,
        ['$http', '$scope', 'common', UserCtrl]);

    function UserCtrl($http, $scope, common) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var logError = getLogFn(controllerId, 'error');
        var logSuccess = getLogFn(controllerId, 'success');

        vm.addItem = addItem;
        vm.deleteItem = deleteItem; 
        vm.title = 'Users';
        vm.users = [];

        activate();

        function activate() {
            common.activateController([getUsers()], controllerId)
                .then(function () {
                    log('Activated Users View');
                });
        }

        //#region Internal Methods        

        function addItem() {
            var user = {
                userName: vm.newUsername,
                email: vm.newUsername + '@splcenter.org', 
                roles: ['user'],
                password: '!1Password'
            };
            log(user);
            $http.post('/api/accounts/create', user).then(function (response) {
                log(response);
                vm.users.unshift(user);
            })
        }

        function deleteItem(user) {
            $http.delete('/api/accounts/user/' + user.id).then(function(response) {
                log(response);
                var idx = vm.users.indexOf(user);
                vm.users.splice(idx, 1)
            });
        }

        function getUsers() {
            $http.get('/api/accounts/users').then(function(response) {
                vm.users = response.data;
                log('vm.users', vm.users, null);
                return vm.users; 
            });
        }
        //#endregion
    }
})();
