
(function () {
    'use strict';

    var controllerId = 'searchCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', searchCtrl]);

    function searchCtrl($scope) {
        var vm = this;

        vm.activate = activate;
        vm.title = 'searchCtrl';

        function activate() {
        }

        //#region Internal Methods        

        //#endregion
    }
})();


//(function () {
//    'use strict';
//
//    var controllerId = 'search';
//    angular.module('app').controller(controllerId,
//    ['$cookieStore', '$cookies', '$scope', '$state', '$stateParams', '$timeout', '$window', '$filter', 'common', 'datacontext', '$http', 'config', 'usSpinnerService', viewmodel]);
//
//    function viewmodel($cookieStore, $cookies, $scope, $state, $stateParams, $timeout, $window, $filter, common, datacontext, $http, config, usSpinnerService) {
//        var vm = this;
//        var getLogFn = common.logger.getLogFn;
//        var log = getLogFn(controllerId);
//        var crmc = $.connection.cRMCHub;
//        var urls = config.urls;
//        var prevSelection = null;
//
//        vm.person = undefined;
//        vm.finish = finish;
//        vm.goBack = goBack;
//        vm.gotoHome = gotoHome;
//        vm.searchText = '';
//        vm.search = search;
//        vm.toggleName = toggleName;
//        vm.names = [];
//
//        vm.filteredNamesCount = 0;
//        vm.filteredNames = [];
//
//        vm.paging = {
//            pageSize: 13,
//            currentPage: 1,
//            maxPagesToShow: 5,
//            pageCount: 5
//        }
//        vm.pageChanged = pageChanged;
//
//        activate();
//
//        function activate() {
//            common.activateController([], controllerId)
//            .then(function () {
//                log('Activated Search View', null, false);
//                $.connection.hub.start().done(function () {
//                    log('hub connection successful', null, false);
//                });
//            });
//            vm.kiosk = $cookies.kiosk || 1;
//        }
//
//        function finish() {
//            crmc.server.addNameToWall(vm.kiosk, vm.person.fullName);
//            $state.go('create.finish')
//                .then(
//                $timeout(function () {
//                    $state.go('welcome');
//                }, 30000)
//                );
//        }
//
//        function goBack() {
//            vm.paging.currentPage = 1;
//            $window.history.back();
//        }
//
//        function gotoHome() {
//            $state.go('welcome');
//        }
//
//        function pageChanged() {
//            vm.person = undefined;
//            if (!vm.paging.currentPage) { return; }
//            getPeople(vm.paging.currentPage, vm.paging.pageSize, vm.searchText);
//        }
//
//        function getPeople(page, size, nameFilter) {
//            usSpinnerService.spin('spinner-1');
//            datacontext.getPersons(page, size, nameFilter).then(function (resp) {
//                vm.names = resp.data;
//                usSpinnerService.stop('spinner-1');
//                return vm.names;
//            });
//        }
//
//        function getPeopleCount(nameFilter) {
//            var fn = nameFilter.split(' ')[0] || '';
//            var ln = nameFilter.split(' ')[1] || fn;
//            if (fn === ln) { fn = '' };
//
//            datacontext.getPersonCount(nameFilter).then(function (resp) {
//                vm.namesFilterCount = resp.data.length;
//            });
//        }
//
//        function search() {
//            $scope.$broadcast('show-errors-check-validity');
//
//            if (vm.searchForm.$invalid) {
//                //                toastr.error('Please correct your search criteria');
//                return;
//            }
//            getPeopleCount(vm.searchText);
//            getPeople(vm.paging.currentPage, vm.paging.pageSize, vm.searchText);
//
//            $state.go('search.step2');
//        }
//
//        function toggleName(name) {
//            if (prevSelection) {
//                prevSelection.$selected = false;
//            }
//            if (prevSelection == name) {
//                name.$selected = false;
//                vm.person = undefined;
//                prevSelection = null;
//            } else {
//                name.$selected = true;
//                vm.person = name;
//                prevSelection = name;
//            }
//        }
//
//    }
//})();
