(function () {
    'use strict';

    var controllerId = 'createCtrl';
    angular.module('app').controller(controllerId,
        ['$rootScope', '$scope', '$state', '$timeout', '$window', 'common', 'datacontext', createCtrl]);

    function createCtrl($rootScope, $scope, $state, $timeout, $window, common, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var arrayMax = Function.prototype.apply.bind(Math.max, null);
        var arrayMin = Function.prototype.apply.bind(Math.min, null);

        vm.blackList = JSON.parse(localStorage.getItem('blacklist')) || [];
        vm.cancel = cancel;
        vm.editItem = undefined;
        vm.goBack = goBack;
        vm.gotoReview = gotoReview;
        vm.lastLetterIsSpace = false;
        vm.person = {
            emailAddress: '',
            firstname: '',
            lastname: ''
        };
        vm.save = save;
        vm.showValidationErrors = false;
        vm.title = 'Add Your Name';

        activate();

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
            startTimer();
        }

        function activate() {
            common.activateController([], controllerId).then(function () {
                log('Activated Create Input View', null, false);
                createValidationWatcher();
                vm.setFocus();
            });
        }

        //#region Internal Methods      
        var createTimer;
        startTimer();

        function cancel() {
            cancelTimer();
            $state.go('welcome');
        }

        function createValidationWatcher() {
            log('creating watcher', vm.person, false);
            $scope.$watchCollection('vm.person', function (newVal, oldVal) {
                if (vm.person) {
                    var valid = validatePerson(vm.person);

                    if (valid) {
                        vm.nameForm.inputFirstName.$setValidity('blacklist', true);
                        vm.nameForm.inputLastName.$setValidity('blacklist', true);
                    } else {
                        vm.nameForm.inputFirstName.$setValidity('blacklist', false);
                        vm.nameForm.inputLastName.$setValidity('blacklist', false);
                    }
                }
            })
        }

        function validatePerson(person) {
            if (person.lastname.length == 0 || person.firstname.length == 0) return true;
            var fullName = person.firstname + ' ' + person.lastname;
            fullName = fullName.toUpperCase();
            var index = vm.blackList.indexOf(fullName);
            //Left for debugging in console window
            log('fullname', fullName.toUpperCase(), false);
            log('indexOf', vm.blackList.indexOf(fullName), false);
            log('index', index, false);

            return index === -1;
        }

        function loadBlackListNames() {
            if (!localStorage.getItem('blacklist')) {
                console.log('loading blacklist from remote storage')
                datacontext.getCensors(true).then(function (data) {
                    data.forEach(function (item) {
                        if (item.word !== null) {
                            vm.blackList.push(item.word.toUpperCase());
                        }
                    });
                    return vm.blackList;
                });
            } else {
                vm.blackList = JSON.parse(localStorage.getItem('blacklist'));
            }
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
            cancelTimer();
            $window.history.back();
            startTimer();
        }

        function gotoReview() {
            cancelTimer();
            if (vm.nameForm.$invalid) {

                vm.showValidationErrors = true;
                toastr.error('Please correct your information');
                return;
            }
            vm.person.firstname = Humanize.titleCase(vm.person.firstname.toLowerCase());
            vm.person.lastname = Humanize.titleCase(vm.person.lastname.toLowerCase());
            vm.person.emailAddress = vm.person.emailAddress.toLowerCase();
            $state.go('create.review');
            startTimer();

        }

        function save() {

            vm.person.fuzzyMatchValue = getFuzzyMatchValue();
            vm.person.dateCreated = moment().format('MM/DD/YYYY HH:mm:ss');
            vm.person.isDonor = false;
            vm.person.isPriority = false;

            datacontext.create('Person', vm.person);

            datacontext.save();
            log('adding new person', vm.person, false);
            vm.person = undefined;
            $rootScope.person = vm.person;
            cancelTimer();
            $state.go('finish');

        }

        function startTimer() {
            log('starting timer', null, false);
            cancelTimer();
            createTimer = $timeout(function () {
                $state.go('welcome');
            }, 30000);
        }

        function cancelTimer() {
            $timeout.cancel(createTimer);
        }

        //#endregion
    }
})();
