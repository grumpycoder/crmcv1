(function () {
    'use strict';

    var controllerId = 'DashboardCtrl';
    angular.module('app').controller(controllerId, ['$scope', 'common', 'datacontext', dashboardCtrl]);

    function dashboardCtrl($scope, common, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.activate = activate;
        vm.title = 'DashboardCtrl';

        vm.sessionCount = 10;
        vm.attendeeCount = 20;
        vm.speakerCount = 14;

        vm.people = [];
        vm.refresh = refresh;

        vm.totalVisitors = 0;
        vm.totalVisitorsToday = 87;
        vm.totalVisitorsThisMonth = 238;

        activate();

        var proxy = $.connection.crmcHub;
        proxy.on('nameAddedToWall', function(kiosk, person) {
            $scope.$apply(function() {
                vm.people.unshift(person);
                vm.people.splice(10, 1)
                vm.totalVisitorsToday += 1; 
            })
            
        });

        function activate() {
            var promises = [getPeople(), getPeopleCount()];
            common.activateController(promises, controllerId)
                .then(function () {
                    log('Activated Dashboard View');
                    $.connection.hub.start().done(function () {
                        console.log('connection started');
                    })
                });
        }

        //#region Internal Methods        

        function getPeople() {
            return datacontext.getPeople(1, 10).then(function (data) {
                vm.people = data.results;
                vm.totalVisitors = data.inlineCount;
                return vm.people;
            });
        }

        function refresh() {
            getPeople();
        }

        function getPeopleCount() {

            datacontext.getPeopleCountByDays(30).then(function (data) {
                vm.totalVisitorsThisMonth = data
            });

            datacontext.getPeopleCountByDays(1).then(function (data) {
                vm.totalVisitorsToday = data;
            });

        }
        //#endregion
    }
})();

//(function () {
//    "use strict";

//    function dashboardCtrl($scope) {

//        $scope.proxy = $.connection.crmcHub;

//        $scope.proxy.client.nameAddedToWall = function (kiosk, person) {
//            console.log('message received', person);
//        }

//        //$.connection.hub.logging = true; 
//        $.connection.hub.start().done(function() {
//            console.log('connection started');
//        }).fail(function() {
//            console.log('could not connect');
//        }); 



////        proxy.client.addMessage = function (message) {
////            console.log('message received ' + message);
////        }



//        $scope.sendMessage = function () {
//            console.log('MESSAGE SENT from dashboard');
//            $scope.proxy.server.sendMessage('test from dashboard');
//        }


//        //$scope.proxy.on('addMessage', function(data) {
//        //    console.log('message received ');
//        //})

//    }

//    angular.module('app').controller('DashboardCtrl', ['$scope', dashboardCtrl])
//}());

