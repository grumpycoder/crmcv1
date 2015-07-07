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


        vm.unlockSettings = unlockSettings;
        var keyCode = '';

        function unlockSettings(key) {
            console.log(key);
            if (keyCode.length > 4) {
                keyCode = key;
            }
            else {
                keyCode += key.toString();
            }

            //if (key === 1) {
            //    keyCode = key;
            //} else {
            //    keyCode += key.toString();
            //}

            if (keyCode === '1212') {
                $state.go('settings');
            }
            console.log(keyCode);
        }

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
