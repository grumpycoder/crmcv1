
(function() {
    'use strict';

    var controllerId = 'finishCtrl';
    angular.module('app').controller(controllerId,
    ['$cookies', '$scope', '$state', '$rootScope', '$timeout', finishCtrl]);

    function finishCtrl($cookies, $scope, $state, $rootScope, $timeout) {
        var vm = this;
        var crmc = $.connection.crmcHub;
        var person = $rootScope.person;
        var kiosk = $cookies.kiosk || 1;

        vm.gotoWelcome = gotoWelcome;
        vm.title = 'finishCtrl';
        

        activate();

        function activate() {
        }

        //#region Internal Methods        

        var timer = $timeout(function () {
            $state.go('welcome');
        }, 15000)

        var timer2 = $timeout(function () {
            if (person) {
                crmc.server.addNameToWall(kiosk, $rootScope.person);
            }
        }, 3000);

        function gotoWelcome() {
            $timeout.cancel(timer);
            $state.go('welcome');
        }
        //#endregion
    }
})();
