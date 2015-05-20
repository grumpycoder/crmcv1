(function () {
    'use strict';

    var controllerId = 'createCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$state', '$window', 'common', 'datacontext', createCtrl]);

    function createCtrl($scope, $state, $window, common, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var arrayMax = Function.prototype.apply.bind(Math.max, null);
        var arrayMin = Function.prototype.apply.bind(Math.min, null);
        var crmc = $.connection.crmcHub;
        var proxy = $.connection.crmcHub;
        
        vm.blackList = [];
        vm.cancel = cancel;
        vm.goBack = goBack;
        vm.gotoReview = gotoReview;
        vm.save = save;
        vm.person = {
            emailAddress: ''
        };
        vm.showValidationErrors = false;
        vm.title = 'Add Your Name';
        vm.lastLetterIsSpace = false;

        activate();

        vm.editItem = undefined;
        vm.setFocus = function (event) {
            vm.editItem = event || vm.nameForm.inputFirstName;
        }

        vm.keyboardInput = function (key) {
            var keyCode = key.currentTarget.outerText
            if (keyCode === 'SPACE') keyCode = ' ';

            if (keyCode === 'DEL') {
                vm.editItem.$setViewValue(vm.editItem.$viewValue.substr(0, vm.editItem.$viewValue.length - 1));
                vm.editItem.$render();
                return;
            }

            if (vm.lastLetterIsSpace) {
                vm.editItem.$setViewValue(vm.editItem.$viewValue + ' ' + keyCode);
                vm.lastLetterIsSpace = false;
            } else {
                vm.editItem.$setViewValue(vm.editItem.$viewValue + keyCode);
            }
            vm.editItem.$render();
        }

        function activate() {
            common.activateController([getBlackList()], controllerId).then(function () {
                log('Activated Create Input View', null, false);
                $.connection.hub.start().done(function () {
                    log('hub connection successful', null, false);
                });
                createValidationWatch();
                vm.setFocus();
                log('blacklist', vm.blacklist, false);
            });

        }

        //#region Internal Methods        
        function cancel() {
            $state.go('welcome');
        }

        function createValidationWatch() {

            $scope.$watch('vm.person.lastname', function (newVal, oldVal) {
                if (vm.person) {
                    var fullName = vm.person.firstname + ' ' + vm.person.lastname;
                    //validateFullName(fullName);
                    if (!validateFullName(fullName)) {
                        vm.nameForm.inputFirstName.$setValidity('valBlacklist', false);
                        vm.nameForm.inputLastName.$setValidity('valBlacklist', false);
                    } else {
                        if (vm.nameForm.inputFirstName) {
                            vm.nameForm.inputFirstName.$setValidity('valBlacklist', true);
                        }
                        if (vm.nameForm.inputLastName) {
                            vm.nameForm.inputLastName.$setValidity('valBlacklist', true);
                        }
                    }
                }
            })
        }

        function getBlackList() {
            //TODO: Load blacklist in localstorage
            var list = []; 
            datacontext.getCensors(true).then(function (data) {
                data.forEach(function (item) {
                    vm.blackList.push(item.word);
                    list.push(item.word);
                });
                $window.myList = list;


                return vm.blackList;
            });
        }

        function getFuzzyMatchValue() {
            var fn = vm.person.firstname;
            var ln = vm.person.lastname;
            var full = fn + ' ' + ln;

            //Check fullname, first and last names and take highest match value
            var matchSet = FuzzySet(vm.blackList);
            var fnScore = matchSet.get(fn, 'useLevenshtein')[0][0];
            var lnScore = matchSet.get(ln, 'useLevenshtein')[0][0];
            var fullScore = matchSet.get(full, 'useLevenshtein')[0][0];

            var scores = [fnScore, lnScore, fullScore];
            var maxScore = arrayMax(scores);

            return maxScore;
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
            vm.person.firstname = Humanize.titleCase(vm.person.firstname.toLowerCase());
            vm.person.lastname = Humanize.titleCase(vm.person.lastname.toLowerCase());
            $state.go('create.review');
        }

        function save() {
            vm.person.fuzzyMatchValue = getFuzzyMatchValue();
            vm.person.dateCreated = moment().format('MM/DD/YYYY HH:mm:ss');
            datacontext.create('Person', vm.person);

            datacontext.save();
            var person = {
                firstname: vm.person.firstname,
                lastname: vm.person.lastname,
                fuzzyMatchValue: vm.person.fuzzyMatchValue,
                dateCreated: vm.person.dateCreated,
                zipCode: vm.person.zipcode
            }
            proxy.server.addNameToWall(vm.kiosk, person);
            vm.person = undefined; 
            $state.go('finish');
        }

        function validateFullName(value) {
            var valid = true;
            if (typeof value === "undefined") {
                value = "";
            }

            if (typeof vm.blackList !== "undefined") {
                for (var i = vm.blackList.length - 1; i >= 0; i--) {
                    var lowerValue = value.toLowerCase();
                    if (lowerValue === vm.blackList[i]) {
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
