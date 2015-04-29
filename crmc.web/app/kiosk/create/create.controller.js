
(function () {
    'use strict';

    var controllerId = 'createCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$state', '$window', createCtrl]);

    function createCtrl($scope, $state, $window) {
        var vm = this;

        vm.cancel = cancel;
        vm.goBack = goBack; 
        vm.gotoReview = gotoReview; 
        vm.title = 'createCtrl';

        function activate() {
        }

        //#region Internal Methods        
        function cancel() {
            $state.go('welcome');
        }

        function goBack() {
            $window.history.back();
        }

        function gotoReview() {
            $state.go('create.review');
        }
        //#endregion
    }
})();
