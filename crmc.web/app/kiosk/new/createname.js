
(function () {
    'use strict';

    var controllerId = 'createName';
    angular.module('app').controller(controllerId,
        ['$scope', '$cookies', '$window', '$state', '$http', '$timeout', createName]);

    function createName($scope, $cookies, $window, $state, $http, $timeout) {
        var vm = this;

        vm.activate = activate;
        vm.title = 'createName';

        function activate() {
        }

        //#region Internal Methods        

        //#endregion
    }
})();


//(function () {
//    'use strict';
//
//    var controllerId = 'createname';
//    angular.module('app').controller(controllerId,
//        ['$cookies', '$cookieStore', '$window', '$scope', '$state', '$http', '$timeout', 'common', 'config', 'datacontext', 'usSpinnerService', viewmodel]);
//
//    function viewmodel($cookies, $cookieStore, $window, $scope, $state, $http, $timeout, common, config, datacontext, usSpinnerService) {
//        var vm = this;
//        var getLogFn = common.logger.getLogFn;
//        var log = getLogFn(controllerId);
//        var crmc = $.connection.cRMCHub;
//        var urls = config.urls;
//        var arrayMax = Function.prototype.apply.bind(Math.max, null);
//        var arrayMin = Function.prototype.apply.bind(Math.min, null);
//
//        vm.blackList = [];
//        vm.cancel = cancel;
//        vm.save = save;
//        vm.goBack = goBack;
//        vm.gotoConfirm = gotoConfirm;
//        vm.showValidationErrors = false;
//
//        activate();
//
//        function activate() {
//            common.activateController([getBlackList()], controllerId)
//                .then(function () {
//                    log('Activated CreateName View', null, false);
//                    $.connection.hub.start().done(function () {
//                        log('hub connection successful', null, false);
//                    });
//                });
//            vm.kiosk = $cookies.kiosk || 1;
//        }
//
//        function getBlackList() {
//            //TODO: someday hold blacklist in localstorage
//            datacontext.getBlackList().then(function (data) {
//                data.forEach(function (item) {
//                    vm.blackList.push(item.word);
//                });
//            });
//        }
//
//        function goBack() {
//            $window.history.back();
//        }
//
//        function cancel() {
//            vm.person = null;
//            $state.go('welcome');
//        }
//
//        function gotoConfirm() {
//            $scope.$broadcast('show-errors-check-validity');
//
//            if (vm.nameForm.$invalid) {
//                vm.showValidationErrors = true;
//                toastr.error('Please correct your information');
//                return;
//            }
//            vm.person.firstname = Humanize.titleCase(vm.person.firstname.toLowerCase());
//            vm.person.lastname = Humanize.titleCase(vm.person.lastname.toLowerCase());
//            $state.go('create.step2');
//        }
//
//        function save() {
//            usSpinnerService.spin('spinner-1');
//            var fuzzyMatchVal = getFuzzyMatchValue();
//
//            vm.person.fuzzyMatchValue = fuzzyMatchVal
//            datacontext.savePerson(vm.person)
//                .then(function () {
//                    log('save success', null, false);
//                    crmc.server.addNameToWall(vm.kiosk, vm.person.fullName);
//                    usSpinnerService.stop('spinner-1');
//                    $state.go('create.finish');
//
//                    $timeout(function () {
//                        $state.go('welcome');
//                    }, 5000);
//                }
//                )
//                .catch(function () {
//                    log('error saving', null, false);
//                });
//        }
//
//        function getFuzzyMatchValue() {
//            var fuzzyMatchValue = 0;
//            var fn = vm.person.firstname;
//            var ln = vm.person.lastname;
//
//            //Check fullname, first and last names and take highest match value
//            vm.blackList.forEach(function (word) {
//                if (word !== null && word !== 'NULL') {
//                    var fnScore = fn.score(word, 0.9);
//                    var lnScore = ln.score(word, 0.9);
//                    var fullScore = (ln + fn).score(word, 0.9);
//                    var scores = [fnScore, lnScore, fullScore];
//
//                    var maxScore = arrayMax(scores);
//                    if (maxScore > fuzzyMatchValue) { fuzzyMatchValue = maxScore }
//                }
//            });
//            return fuzzyMatchValue;
//        }
//
//
//    }
//})();
