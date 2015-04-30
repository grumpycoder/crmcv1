(function () {
    'use strict';

    var controllerId = 'createCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$state', '$window', 'common', 'datacontext', createCtrl]);

    function createCtrl($scope, $state, $window, common, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var crmc = $.connection.cRMCHub;

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
                $.connection.hub.start().done(function () {
                    log('hub connection successful', null, false);
                });
                createValidationWatch();
            });
        }

        //#region Internal Methods        
        function cancel() {
            $state.go('welcome');
        }

        function createValidationWatch() {
            $scope.$watch('vm.person.lastname', function (newVal, oldVal) {
                if (vm.person) {
                    //TODO: add fullname to model
                    var fullName = vm.person.firstname + ' ' + vm.person.lastname;
                    validateFullName(fullName);
                    if (!validateFullName(fullName)) {
                        vm.nameForm.inputFirstName.$setValidity('valBlacklist', false);
                        vm.nameForm.inputLastName.$setValidity('valBlacklist', false);
                    } else {
                        vm.nameForm.inputFirstName.$setValidity('valBlacklist', true);
                        vm.nameForm.inputLastName.$setValidity('valBlacklist', true);
                    }
                }
            })
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

            crmc.server.addNameToWall(vm.kiosk, vm.person.firstname + ' ' + vm.person.lastname);
            $state.go('finish');
        }

        function validateFullName(value) {
            var valid = true;

            if (typeof value === "undefined") {
                value = "";
            }

            if (typeof vm.blackList !== "undefined") {
                for (var i = vm.blackList.length - 1; i >= 0; i--) {
                    if (value === vm.blackList[i]) {
                        valid = false;
                        break;
                    }
                }
            }
            return valid;
        }

        //#endregion
    }
})();
