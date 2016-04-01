
(function () {
    'use strict';

    var controllerId = 'finishCtrl';
    angular.module('app').controller(controllerId,
        ['common', '$state', '$timeout', 'datacontext', finishCtrl]);

    function finishCtrl(common, $state, $timeout, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var crmc = $.connection.crmcHub;
        var person = localStorage.getItem('currentPerson'); // $rootScope.person;
        var kiosk = localStorage.getItem('kiosk');

        vm.gotoWelcome = gotoWelcome;
        var timer;
        var disconnectTimer;

        vm.countDown_tick = 1;

        $.connection.hub.disconnected(function () {
            disconnectTimer = $timeout(function () {
                log('Trying to reconnect to hub', null, false);
                $.connection.hub.start();
            }, 5000); // Restart connection after 5 seconds.
        });

        activate();

        var countDownWatch = function () {

            if (vm.countDown_tick <= 0) {
                $state.go('welcome');
            } else {
                    vm.countDown_tick--;
                    $timeout(countDownWatch, 1000);
            }
        };

        function activate() {
            common.activateController([loadConfigurationOptions()], controllerId).then(function () {
                $.connection.hub.start().done(function () {
                    log('hub connection successful', null, false);
                    if (person) {
                        log('sending to wall position' + kiosk, JSON.parse(person), false);
                        crmc.server.addNameToWall(kiosk, JSON.parse(person));
                    }
                });

                vm.countDown_tick = vm.config.newItemOnScreenDelay;

                countDownWatch();
            });
        }

        //#region Internal Methods        

        function gotoWelcome() {
            $timeout.cancel(timer);
            $timeout.cancel(disconnectTimer);
            $state.go('welcome');
        }

        function loadConfigurationOptions() {
            return datacontext.getAppSettings().then(function (response) {
                return vm.config = response;
            }, function () {
                logError('Unable to get configuration settings');
            });
        }


        //#endregion
    }
})();
