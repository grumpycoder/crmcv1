
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

        function activate() {
            var promises = [getPeople(), getPeopleCount()];
            common.activateController(promises, controllerId)
                .then(function () { log('Activated Dashboard View'); });
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

            datacontext.getPeopleCountByDays(30).then(function(data) {
                vm.totalVisitorsThisMonth = data
            });

            datacontext.getPeopleCountByDays(1).then(function(data) {
                vm.totalVisitorsToday = data; 
            });

        }
        //#endregion
    }
})();
