(function () {
    'use strict';

    var controllerId = 'createCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', '$state', '$timeout', '$window', 'common', 'datacontext', createCtrl]);

    function createCtrl($scope, $state, $timeout, $window, common, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var arrayMax = Function.prototype.apply.bind(Math.max, null);
        var arrayMin = Function.prototype.apply.bind(Math.min, null);

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
        var timer;

        activate();

        vm.setFocus = function (event) {
            vm.editItem = event || vm.nameForm.inputFirstName;
        }

        vm.keyboardInput = function (key) {
            ResetTimer();
            var keyCode = key.currentTarget.outerText;
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

        function ResetTimer() {
            $timeout.cancel(timer);
            timer = $timeout(function () {
                $state.go('welcome');
            }, 30000);
        }

        vm.blackList = [];
        function activate() {
            common.activateController([loadBlacklist()], controllerId).then(function () {
                log('Activated Create Input View', null, false);
                createValidationWatcher();
                vm.setFocus();
                ResetTimer();
            });
        }

        function loadBlacklist() {
            var list = [];
            datacontext.getCensors(true).then(function (data) {
                var list = [];
                if (data.length > 0) {
                    data.forEach(function (item) {
                        if (item.word !== null) list.push(item.word.replace(/ /g, "").toUpperCase());
                    });
                    vm.blackList = list;
                }
            });
        }


        //#region Internal Methods      

        function cancel() {
            $timeout.cancel(timer);
            $state.go('welcome');
        }

        function createValidationWatcher() {
            log('creating watcher', vm.person, false);

            $scope.$watch('vm.person.firstname',
                function (newValue, oldValue) {
                    if (newValue !== undefined) {
                        if (newValue.length > 0) vm.nameForm.inputFirstName.$setValidity('blacklist', !checkArray(newValue.toUpperCase(), vm.blackList));
                    }
                });

            $scope.$watch('vm.person.lastname',
                function (newValue, oldValue) {
                    if (newValue !== undefined) {
                        if (newValue.length > 0) vm.nameForm.inputLastName.$setValidity('blacklist', !checkArray(newValue.toUpperCase(), vm.blackList));
                    }
                });

            $scope.$watchCollection('vm.person',
                function (newValue, oldValue) {
                    var name = newValue.firstname + newValue.lastname;
                    name = name.replace(/ /g, ""); 
                    if (name.length > 0) {
                        var valid = !checkArray(name.toUpperCase(), vm.blackList);
                        vm.nameForm.inputLastName.$setValidity('blacklist', valid);
                    }
                });

        }

        function validatePerson(person) {



            //if (person.lastname.length === 0 && person.firstname.length === 0) return true;
            //if (person.lastname === undefined && person.firstname === undefined) return true;

            if (person.firstname !== undefined) {
                if (person.firstname.length !== 0) {

                    if (checkArray(person.firstname.toUpperCase(), vm.blackList)) {
                        console.log('found firstname');
                        return false;
                    } else {
                        return true;
                    }
                    //var f = vm.blackList.indexOf(person.firstname.toUpperCase());
                    //if (f > 0) {
                    //    return f === -1;

                    //}
                }
            }

            if (person.lastname !== undefined) {
                if (person.lastname.length !== 0) {
                    console.log('checking lastname');
                    if (checkArray(person.lastname.toUpperCase(), vm.blackList)) {
                        console.log('found lastname');
                        return false;
                    } else {
                        return true;
                    }
                    //var f = vm.blackList.indexOf(person.firstname.toUpperCase());
                    //if (f > 0) {
                    //    return f === -1;

                    //}
                }
            }

            //if (person.lastname !== undefined) {
            //    if (person.lastname.length !== 0) {
            //        if (checkArray(person.lastname.toUpperCase(), vm.blackList)) {
            //            console.log('found lastname');
            //            return false;
            //        } else {
            //            return true;
            //        }; 

            //        //var l = vm.blackList.indexOf(person.lastname.toUpperCase());
            //        //if (l > 0) {
            //        //    return l === -1;
            //        //}
            //    }
            //}

            //if (person.firstname !== undefined && person.lastname !== undefined) {
            //    var fullname = person.firstname + person.lastname; 
            //    if (checkArray(fullname.toUpperCase(), vm.blackList)) {
            //        console.log('found fullname');
            //        return false;
            //    } else {
            //        return true;
            //    }; 

            //    //var index = vm.blackList.indexOf(fullname.toUpperCase());
            //    //return index === -1;
            //}

            //var fullName = person.firstname + person.lastname;
            //Left for debugging in console window
            //log('fullname', fullName.toUpperCase(), false);
            //log('indexOf', vm.blackList.indexOf(fullName), false);
            //log('index', index, false);

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
            ResetTimer();
            $window.history.back();
        }

        function gotoReview() {
            ResetTimer();
            if (vm.nameForm.$invalid) {
                vm.showValidationErrors = true;
                toastr.error('Please correct your information');
                return;
            }
            vm.person.firstname = Humanize.titleCase(vm.person.firstname.toLowerCase());
            vm.person.lastname = Humanize.titleCase(vm.person.lastname.toLowerCase());
            vm.person.emailAddress = vm.person.emailAddress.toLowerCase();
            $state.go('create.review');
        }

        function save() {
            $timeout.cancel(timer);
            vm.person.fuzzyMatchValue = getFuzzyMatchValue();
            vm.person.dateCreated = moment().format('MM/DD/YYYY HH:mm:ss');
            vm.person.isDonor = false;
            vm.person.isPriority = false;

            datacontext.create('Person', vm.person);
            datacontext.save().then(function () {
                log('Successfully saved new person', vm.person, false);
            });

            localStorage.setItem('currentPerson', JSON.stringify(vm.person));
            vm.person = undefined;
            $state.go('finish');
        }

        function checkArray(str, arr) {
            for (var i = 0; i < arr.length; i++) {
                if (str.match((".*" + arr[i].trim() + ".*").replace(" ", ".*"))) {
                    console.log('found match', arr[i]);
                    return true;
                }
            }
            return false;
        }

        //#endregion
    }
})();
