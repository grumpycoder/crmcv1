
(function () {
    'use strict';

    var controllerId = 'finishCtrl';
    angular.module('app').controller(controllerId,
    ['common', '$state', '$timeout', finishCtrl]);

    function finishCtrl(common, $state, $timeout) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var crmc = $.connection.crmcHub;
        var person = localStorage.getItem('currentPerson'); // $rootScope.person;
        var kiosk = localStorage.getItem('kiosk');

        vm.gotoWelcome = gotoWelcome;
        var timer;
        var disconnectTimer;

        $.connection.hub.disconnected(function () {
            disconnectTimer = $timeout(function () {
                log('Trying to reconnect to hub', null, false);
                $.connection.hub.start();
            }, 5000); // Restart connection after 5 seconds.
        });

        activate();

        function activate() {
            common.activateController([], controllerId).then(function () {
                $.connection.hub.start().done(function () {
                    log('hub connection successful', null, false);
                });

                timer = $timeout(function () {
                    if (person) {
                        log('sending to wall position' + kiosk, JSON.parse(person), false);
                        crmc.server.addNameToWall(kiosk, JSON.parse(person));
                    }
                    $state.go('welcome');
                }, 3000);
            });
        }

        //#region Internal Methods        

        function gotoWelcome() {
            $timeout.cancel(timer);
            $timeout.cancel(disconnectTimer);
            $state.go('welcome');
        }

        //#endregion
    }
})();
