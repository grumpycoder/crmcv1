
(function () {
    'use strict';


    var controllerId = 'settings';
    angular.module('app').controller(controllerId,
        ['$scope', 'common', 'datacontext', viewmodel]);

    function viewmodel($scope, common, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var logError = getLogFn(controllerId, 'error');
        var logSuccess = getLogFn(controllerId, 'success');
        var crmc = $.connection.crmcHub;

        vm.availableKiosks = ['1', '2', '3', '4'];
        vm.config = {};
        vm.getConfigVolume = getConfigVolume;
        vm.save = save;
        vm.selectedKiosk = {};
        vm.title = 'Settings';

        activate();

        function activate() {
            common.activateController([loadConfigurationOptions()], controllerId).then();
            createKioskWatcher();
            vm.selectedKiosk = localStorage.getItem('kiosk') ? localStorage.getItem('kiosk') : 1;
            $.connection.hub.start().done(function () {
                log('hub connection successful', null, false);
            });
        }

        function createKioskWatcher() {
            log('create watcher for kiosk change', null, false);
            $scope.$watch('vm.selectedKiosk', function () {
                localStorage.setItem('kiosk', vm.selectedKiosk);
                logSuccess('Kiosk local storage set', vm.selectedKiosk, true);
            })
        }

        function loadConfigurationOptions() {
            return datacontext.getAppSettings().then(function (response) {
                vm.config = response;
                log('config', vm.config, null);
            }, function () {
                logError('Unable to get configuration settings');
            });
        }

        function save() {
            datacontext.save().then(function () {
                log('Saved Settings', vm.config, null);
                crmc.server.configSettingsSaved();
            });
        }

        function getConfigVolume() {
            return Math.round(vm.config.volume * 100);
        }

    }
})();
