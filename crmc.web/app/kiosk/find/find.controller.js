
(function () {
    'use strict';

    var controllerId = 'findCtrl';
    angular.module('app').controller(controllerId,
        ['$cookies', '$cookieStore', '$rootScope', '$scope', '$state', '$window', 'common', 'config', 'datacontext', 'usSpinnerService', findCtrl]);

    function findCtrl($cookies, $cookieStore, $rootScope, $scope, $state, $window, common, config, datacontext, usSpinnerService) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var prevSelection = null;
        var crmc = $.connection.crmcHub;

        vm.cancel = cancel;
        vm.finish = finish;
        vm.goBack = goBack;
        vm.kiosk = 1; 
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

        vm.editItem = undefined;
        vm.setFocus = function (event) {
            vm.editItem = event;
        }

        vm.lastLetterIsSpace = false;

        vm.keyboardInput = function (key) {
            vm.showValidationErrors = true;
            var keyCode = key.currentTarget.outerText
            if (keyCode === 'SPACE') {
                vm.lastLetterIsSpace = true;
                return;
            }

            if (keyCode === 'DEL') {
                vm.editItem.$setViewValue(vm.editItem.$viewValue.substr(0, vm.editItem.$viewValue.length - 1));
                vm.editItem.$render();
                return;
            }

            vm.editItem = vm.editItem || vm.searchForm.inputSearchText;
            if (vm.lastLetterIsSpace) {
                vm.editItem.$setViewValue(vm.editItem.$viewValue + ' ' + keyCode);
                vm.lastLetterIsSpace = false;
            } else {
                vm.editItem.$setViewValue(vm.editItem.$viewValue + keyCode);
            }
            vm.editItem.$render();
        }

        activate();

        function activate() {
            common.activateController([], controllerId)
                 .then(function () {
                     log('Activated Find Person View', null, false);
                     $.connection.hub.start().done(function () {
                         log('hub connection successful', null, false);
                     });
                 });
            vm.kiosk = $cookies.kiosk;
        }

        //#region Internal Methods        
        function cancel() {
            $state.go('welcome');
        }

        function getPeople(forceRefresh) {
            usSpinnerService.spin('spinner-1');
            datacontext.getPeople(vm.paging.currentPage, vm.paging.pageSize, vm.peopleSearch).then(function (data) {
                vm.people = data.results;
                vm.peopleFilteredCount = data.inlineCount;
                usSpinnerService.stop('spinner-1');
            })

        }

        function goBack() {
            $window.history.back();
        }

        function finish() {
            var person = {
                firstname: vm.person.firstname,
                lastname: vm.person.lastname,
                fuzzyMatchValue: vm.person.fuzzyMatchValue,
                dateCreate: vm.person.dateCreate
            }
            crmc.server.addNameToWall(vm.kiosk, person);
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
