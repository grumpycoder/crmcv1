
(function () {
    'use strict';

    var controllerId = 'findCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$state', findCtrl]);

    function findCtrl($scope, $state) {
        var vm = this;
        var prevSelection = null;

        vm.cancel = cancel; 
        vm.finish = finish;
        vm.people = [
            { firstname: 'Mark', lastname: 'Lawrence', zipCode: '11111' },
            { firstname: 'Mark2', lastname: 'Lawrence2', zipCode: '22222' },
            { firstname: 'Mark3', lastname: 'Lawrence3', zipCode: '33333' }
        ];
        vm.person = undefined;
        vm.title = 'findCtrl';
        vm.paging = {
            pageSize: 13,
            currentPage: 1,
            maxPagesToShow: 5,
            pageCount: 5
        }
        vm.pageChanged = pageChanged;
        vm.search = search;
        vm.searchText = '';
        vm.toggleName = toggleName;

        function activate() {
        }

        //#region Internal Methods        
        function cancel() {
            $state.go('welcome');
        }

        function finish() {
            $state.go('finish');
        }

        function pageChanged() {
            vm.person = undefined;
            if (!vm.paging.currentPage) { return; }
            //Get next page of people
            //getPeople(vm.paging.currentPage, vm.paging.pageSize, vm.searchText);
        }

        function search() {
            //Load people and go to result view
            console.log(vm.searchText);
          
            $state.go('find.searchresult');
        }

        function toggleName(person) {
            if (prevSelection) {
                prevSelection.$selected = false;
            }
            if (prevSelection == person) {
                person.$selected = false;
                vm.person = undefined;
                prevSelection = null;
            } else {
                person.$selected = true;
                vm.person = person;
                prevSelection = person;
            }
        }


        //#endregion
    }
})();
