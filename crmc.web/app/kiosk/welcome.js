(function () {
    'use strict';

    var controllerId = 'welcome';
    angular.module('app').controller(controllerId,
                  ['common', '$scope', '$state', 'usSpinnerService', 'datacontext', welcome]);

    function welcome(common, $scope, $state, usSpinnerService, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.activate = activate;
        vm.title = 'welcome';
        vm.gotoCreate = gotoCreate;
        vm.gotoSearch = gotoSearch;


        vm.unlockSettings = unlockSettings;
        var keyCode = '';

        function unlockSettings(key) {
            if (keyCode.length > 4) {
                keyCode = key;
            }
            else {
                keyCode += key.toString();
            }

            if (keyCode === '1212') {
                $state.go('settings');
            }
            log('keyCode', keyCode, false);
        }

        activate();


        function activate() {
            common.activateController([loadBlacklist()], controllerId).then(function () {
                log('Activated welcome view', null, false);
            });

        }

        function loadBlacklist() {
            var list = [];
            datacontext.getCensors(true).then(function (data) {
                if (data.length > 0) {
                    data.forEach(function(item) {
                        if (item.word !== null) list.push(item.word.replace(/ /g, "").toUpperCase());
                    });
                    log('loading blacklist to local storage', null, false);
                    localStorage.setItem('blacklist', JSON.stringify(list));
                } else {
                    log('remote blacklist was empty', null, false);
                }
            });
        }

        function gotoCreate() {
            $state.go('create.inputname');
        }

        function gotoSearch() {
            $state.go('find.search');
        }
    }
})();
