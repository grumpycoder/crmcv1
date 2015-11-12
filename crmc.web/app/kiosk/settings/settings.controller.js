
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

        vm.title = 'Settings';
        vm.config = {};
        vm.availableKiosks = ['1', '2', '3', '4'];

        vm.save = save;
        vm.saveKiosk = saveKiosk;
        vm.selectedKiosk = {};
        vm.getConfigVolume = getConfigVolume;

        activate();

        function activate() {
            common.activateController([getConfigOptions()], controllerId).then(onEveryChange);
            vm.selectedKiosk = localStorage.getItem('kiosk') ? localStorage.getItem('kiosk') : 1;
            $.connection.hub.start().done(function () {
                log('hub connection successful', null, false);
            });
        }

        function onEveryChange() {
            // Begin observing model for changes
            //            Object.observe(vm.config.volume, configObserver);
            Object.observe(vm, kioskObserver);
        }

        function getConfigOptions() {
            return datacontext.getAppSettings().then(function (response) {
                vm.config = response;
            }, function () {
                logError('Unable to get configuration settings');
            });
        }

        // Set up our observer
        function configObserver(changes) {
            common.debouncedThrottle(controllerId, saveConfiguration, 1000);
        }

        function kioskObserver(changes) {
            common.debouncedThrottle(controllerId, saveKiosk, 500);
        }

        function saveConfiguration() {
            save();
        }

        function save() {
            datacontext.save().then(function () {
                log('Saved Settings');
                crmc.server.configSettingsSaved();
            });
        }

        function saveKiosk() {
            localStorage.setItem('kiosk', vm.selectedKiosk);
            logSuccess('Saved kiosk', null, true);
        }

        function getConfigVolume() {
            return Math.round(vm.config.volume * 100);
        }

    }
})();
