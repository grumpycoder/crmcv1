
(function () {
    'use strict';

    var controllerId = 'UserCtrl';
    angular.module('app').controller(controllerId,
        ['$http', '$modal', '$scope', 'common', 'config', UserCtrl]);

    function UserCtrl($http, $modal, $scope, common, config) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var logError = getLogFn(controllerId, 'error');
        var logSuccess = getLogFn(controllerId, 'success');
        var keyCodes = config.keyCodes;
        var applyFilter = function () { };

        vm.addItem = addItem;
        vm.availableRoles = [];
        vm.currentEdit = {};
        vm.cancelEdit = cancelEdit;
        vm.clearInput = clearInput;
        vm.deleteItem = deleteItem;
        vm.editItem = editItem;
        vm.filteredUsers = [];
        vm.refresh = refresh;
        vm.saveItem = saveItem;
        vm.search = search;
        vm.title = 'Users';
        vm.users = [];
        vm.usersFilter = usersFilter;
        vm.userSearch = '';

        activate();

        function activate() {
            common.activateController([getUsers(), getAvailableRoles()], controllerId)
                .then(function () {
                    applyFilter = common.createSearchThrottle(vm, 'users');
                    if (vm.userSearch) { applyFilter(true); }
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
            $http.post('/api/accounts/create', user).then(function (response) {
                user.id = response.data.id; 
                logSuccess('Created user ' + user.userName);
                vm.filteredUsers.unshift(user);
                vm.newUsername = ''; 
            })
        }

        function cancelEdit(id) {
            vm.currentEdit[id] = false;
        }

        function clearInput($event) {
            if ($event.keyCode === keyCodes.esc) {
                vm.newUsername = '';
            }
        }

        function deleteItem(user) {
            $http.delete('/api/accounts/user/' + user.id).then(function (response) {
                var idx = vm.users.indexOf(user);
                vm.filteredUsers.splice(idx, 1)
            });
        }

        function editItem(item) {
            vm.currentEdit[item.id] = true;
            vm.itemToEdit = angular.copy(item);
        }

        function getAvailableRoles() {
            $http.get('/api/accounts/roles').then(function (response) {
                vm.availableRoles = response.data;
                return vm.availableRoles;
            });
        }

        function getUsers() {
            $http.get('/api/accounts/users').then(function (response) {
                vm.users = vm.filteredUsers = response.data;
                applyFilter();
                return vm.users;
            });
        }

        function refresh() {
            getUsers();
        }

        function saveItem(user) {

            vm.currentEdit[user.id] = false;
            var roles = [];
            
            _.forEach(vm.itemToEdit.roles, function (role) {
                roles.push(role.name);
            });

            $http.put('/api/accounts/user/' + user.id + '/roles', roles).then(function (response) {
                user.roles = roles;
            });

        }

        function search($event) {
            if ($event.keyCode === keyCodes.esc) {
                vm.userSearch = '';
                applyFilter(true);
            } else {
                applyFilter();
            }
        }

        function usersFilter(user) {
            var textContains = common.textContains;
            var searchText = vm.userSearch;
            var isMatch = searchText ?
                textContains(user.userName, searchText)
                : true;
            return isMatch;
        }
        //#endregion
    }
})();
