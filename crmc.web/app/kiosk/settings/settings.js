
(function () {
    'use strict';


    var controllerId = 'settings';
    angular.module('app').controller(controllerId,
        ['$cookies', '$cookieStore', 'common', 'datacontext', viewmodel]);

    function viewmodel($cookies, $cookieStore, common, datacontext) {

        var getLogFn = common.logger.getLogFn;
        var logError = getLogFn(controllerId, 'error');
        var logSuccess = getLogFn(controllerId, 'success');

        var vm = this;

        vm.title = 'configuration';
        vm.config = {};
        vm.availableKiosks = ['1', '2', '3', '4'];

        vm.save = save;
        vm.saveKiosk = saveKiosk;
        vm.selectedKiosk = {};
        vm.getConfigVolume = getConfigVolume;

        activate();

        function activate() {
            common.activateController([getConfigOptions()], controllerId).then(onEveryChange);
            vm.selectedKiosk = $cookies.kiosk; 
        }

        function onEveryChange() {
            // Begin observing model for changes
            Object.observe(vm.config, configObserver);
            Object.observe(vm, kioskObserver);
        }

        function getConfigOptions() {
            return datacontext.getWallConfig().then(function (response) {
                vm.config = response.data;
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
            datacontext.saveConfigValues(vm.config).then(function () {
                logSuccess('Saved sucessfully', null, true);

            }, function () {
                logError('Save failed', null, true);
            });
        }

        function saveKiosk() {
            $cookies.kiosk = vm.selectedKiosk; 
            logSuccess('Saved kiosk', null, true);
        }

        function getConfigVolume() {
            return Math.round(vm.config.volume * 100);
        }

    }
})();
