
(function () {
    'use strict';

    var controllerId = 'finishCtrl';
    angular.module('app').controller(controllerId,
    ['common', '$scope', '$state', '$rootScope', '$timeout', finishCtrl]);

    function finishCtrl(common, $scope, $state, $rootScope, $timeout) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var crmc = $.connection.crmcHub;
        var person = $rootScope.person;
        var kiosk = localStorage.getItem('kiosk');

        vm.gotoWelcome = gotoWelcome;
        vm.title = 'finishCtrl';


        $.connection.hub.disconnected(function () {
            setTimeout(function () {
                log('Trying to reconnect to hub', null, false);
                $.connection.hub.start();
            }, 5000); // Restart connection after 5 seconds.
        });


        activate();

        function activate() {
            $.connection.hub.start().done(function () {
                log('hub connection successful', null, false);
            });
        }

        //#region Internal Methods        

        var finishTimer = $timeout(function() {
            $state.go('welcome');
        }, 3000);

        var timer2 = $timeout(function () {
            if (person) {
                crmc.server.addNameToWall(kiosk, $rootScope.person);
            }
        }, 1000);

        function gotoWelcome() {
            $timeout.cancel(finishTimer);
            $state.go('welcome');
        }
        //#endregion
    }
})();
