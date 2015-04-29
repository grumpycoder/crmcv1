(function () {
    'use strict';

    var controllerId = 'welcome';
    angular.module('app').controller(controllerId,
                  ['$scope', '$state', welcome]);

    function welcome($scope, $state) {
        var vm = this;

        vm.activate = activate;
        vm.title = 'welcome';
        vm.gotoCreate = gotoCreate;
        vm.gotoSearch = gotoSearch;

        activate();

        function activate() {
//            usSpinnerService.spin('spinner-1');
        }

        function gotoCreate() {
            $state.go('create.inputname');
        }

        function gotoSearch() {
            $state.go('find.search');
        }
    }
})();
