
(function () {
    'use strict';

    var controllerId = 'DashboardCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', dashboardCtrl]);

    function dashboardCtrl($scope) {
        var vm = this;

        vm.activate = activate;
        vm.title = 'DashboardCtrl';

        function activate() {
        }

        //#region Internal Methods        

        //#endregion
    }
})();
