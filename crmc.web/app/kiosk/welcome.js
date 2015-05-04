(function () {
    'use strict';

    var controllerId = 'welcome';
    angular.module('app').controller(controllerId,
                  ['$scope', '$state', 'usSpinnerService', welcome]);

    function welcome($scope, $state, usSpinnerService) {
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
            $state.go('settings');
        }
    }
})();
