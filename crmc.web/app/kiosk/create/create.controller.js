(function () {
    'use strict';

    var controllerId = 'createCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$state', '$window', 'common', 'datacontext', createCtrl]);

    function createCtrl($scope, $state, $window, common, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.blackList = [];
        vm.cancel = cancel;
        vm.goBack = goBack;
        vm.gotoReview = gotoReview;
        vm.save = save; 
        vm.person = undefined;
        vm.showValidationErrors = false;
        vm.title = 'Add Your Name';

        activate();

        function activate() {
            common.activateController([getBlackList()], controllerId).then(function () {
                log('Activated Create Input View', null, false);
            });
        }

        //#region Internal Methods        
        function cancel() {
            $state.go('welcome');
        }

        function getBlackList() {
            //TODO: Load blacklist in localstorage
            datacontext.getCensors().then(function (data) {
                data.forEach(function (item) {
                    vm.blackList.push(item.word);
                });
                return vm.blackList;
            });
        }

        function goBack() {
            $window.history.back();
        }

        function gotoReview() {
            if (vm.nameForm.$invalid) {
                vm.showValidationErrors = true;
                toastr.error('Please correct your information');
                return;
            }
            $state.go('create.review');
        }

        function save() {
            datacontext.create('Person', vm.person);
            datacontext.save();
            log('Saved person', null, false);
            $state.go('finish');
        }

        //#endregion
    }
})();
