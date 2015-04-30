
(function () {
    'use strict';

    var controllerId = 'findCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$state', '$window', 'common', 'config', 'datacontext', findCtrl]);

    function findCtrl($scope, $state, $window, common, config, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var prevSelection = null;

        vm.cancel = cancel;
        vm.finish = finish;
        vm.goBack = goBack; 
        vm.people = [];
        vm.person = undefined;
        vm.title = 'Find Your Name';
        vm.peopleFilteredCount = 0; 
        vm.paging = {
            pageSize: 13,
            currentPage: 1,
            maxPagesToShow: 5,
            pageCount: 5
        }
        vm.pageChanged = pageChanged;
        vm.search = search;
        vm.showValidationErrors = false;
        vm.peopleSearch = '';
        vm.toggleName = toggleName;

        activate();

        function activate() {
            common.activateController([], controllerId)
                 .then(function () {
//                     log('Activated People View', null, false);
                 });

        }

        //#region Internal Methods        
        function cancel() {
            $state.go('welcome');
        }

        function getPeople(forceRefresh) {
            datacontext.getPeople(vm.paging.currentPage, vm.paging.pageSize, vm.peopleSearch).then(function(data) {
                vm.people = data.results;
                vm.peopleFilteredCount = data.inlineCount;
            })

        }

        function goBack() {
            $window.history.back();
        }

        function finish() {
            //TODO: Send vm.person name to hub method
            $state.go('finish');
        }

        function pageChanged() {
            vm.person = undefined;
            if (!vm.paging.currentPage) { return; }
            //Get next page of people
            getPeople(vm.paging.currentPage, vm.paging.pageSize, vm.searchText);
        }

        function search() {
            //Load people and go to result view
            if (vm.searchForm.$invalid) {
                vm.showValidationErrors = true;
                return;
            }

            getPeople();
            $state.go('find.searchresult');
        }

        function toggleName(person) {
            if (prevSelection) {
                prevSelection.$selected = false;
            }
            if (prevSelection === person) {
                person.$selected = false;
                vm.person = undefined;
                prevSelection = null;
            } else {
                person.$selected = true;
                vm.person = person;
                prevSelection = person;
            }
            log('selected', vm.person, null);
        }


        //#endregion
    }
})();
