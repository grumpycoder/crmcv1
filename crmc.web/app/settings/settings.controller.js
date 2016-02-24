
(function () {
    'use strict';


    var controllerId = 'SettingsCtrl';
    angular.module('app').controller(controllerId,
        ['$cookies', '$cookieStore', '$scope', 'common', 'datacontext', viewmodel]);

    function viewmodel($cookies, $cookieStore, $scope, common, datacontext) {
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
            common.activateController([getConfigOptions()], controllerId).then();
            vm.selectedKiosk = localStorage.getItem('kiosk') ? localStorage.getItem('kiosk') : 1;
            createKioskWatcher();

            $.connection.hub.start().done(function () {
                log('hub connection successful', null, false);
            });
        }

        function createKioskWatcher() {
            log('create watcher for kiosk change', null, false);
            $scope.$watch('vm.selectedKiosk', function () {
                localStorage.setItem('kiosk', vm.selectedKiosk);
                logSuccess('Kiosk local storage set [' + vm.selectedKiosk + ']');
            })
        }

        function getConfigOptions() {
            return datacontext.getAppSettings().then(function (response) {
                vm.config = response;
            }, function () {
                logError('Unable to get configuration settings');
            });
        }

        function saveConfiguration() {
            save();
        }

        function save() {
            datacontext.save().then(function () {
                log('Saved Settings', vm.config, false);
                log('Saved Settings', null, true);
                crmc.server.configSettingsSaved();
            });
        }

        function getConfigVolume() {
            return Math.round(vm.config.volume * 100);
        }

    }
})();
