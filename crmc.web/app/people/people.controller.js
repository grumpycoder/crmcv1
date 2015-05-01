
(function () {
    'use strict';

    var controllerId = 'PeopleCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', peopleCtrl]);

    function peopleCtrl($scope) {
        var vm = this;

        vm.activate = activate;
        vm.title = 'peopleCtrl';

        function activate() {
        }

        //#region Internal Methods        

        //#endregion
    }
})();
