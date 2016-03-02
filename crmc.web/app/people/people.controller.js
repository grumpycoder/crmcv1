(function () {
    'use strict';
    var controllerId = 'PeopleCtrl';
    angular.module('app').controller(controllerId, ['$modal', '$scope', 'appSpinner', 'common', 'config', 'datacontext', viewmodel]);

    function viewmodel($modal, $scope, appSpinner, common, config, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var logError = getLogFn(controllerId, 'error');
        var logSuccess = getLogFn(controllerId, 'success');
        var keyCodes = config.keyCodes;

        vm.addPerson = addPerson;
        vm.clearSearch = clearSearch;
        vm.daysFilter = '';
        vm.fuzzyMatchValue = '';
        vm.deletePerson = deletePerson;
        vm.isPriority = '';
        vm.isLocal = '';
        vm.orderByField = 'lastname';
        vm.reverseSort = false;
        vm.people = [];
        vm.peopleCount = 0;
        vm.peopleFilteredCount = 0;
        vm.peopleSearch = '';
        vm.paging = {
            currentPage: 1,
            maxPagesToShow: 10,
            pageSize: 15
        };
        vm.pageChanged = pageChanged;
        vm.refresh = refresh;
        vm.search = search;
        vm.sort = sort;
        vm.title = 'People';
        vm.updatePerson = updatePerson;

        Object.defineProperty(vm.paging, 'pageCount', {
            get: function () {
                return Math.floor(vm.peopleFilteredCount / vm.paging.pageSize) + 1;
            }
        });

        activate();

        function activate() {
            common.activateController([getPeople()], controllerId)
                .then(function () {
                    log('Activated People View');
                });
        }

        $.connection.hub.start().done(function() {
            console.log('connection started');
        });

        var proxy = $.connection.crmcHub;

        function addPerson(size) {

            var newPerson = datacontext.create('Person');
    
            var modalInstance = $modal.open({
                templateUrl: 'app/people/editPerson.html',
                controller: function ($scope, $modalInstance, person, datacontext) {
                    //$scope.person = newPerson;

                    $scope.person = {
                        firstname: person.firstname,
                        lastname: person.lastname,
                        emailAddress: person.emailAddress,
                        accountId: person.accountId, 
                        zipcode: person.zipcode,
                        isDonor: person.isDonor,
                        isPriority: person.isPriority,
                        fuzzyMatchValue: 0
                    };
                    
                    $scope.save = function () {
                        person.firstname = $scope.person.firstname;
                        person.lastname = $scope.person.lastname;
                        person.email = $scope.person.email;
                        person.accountId = $scope.person.accountId;
                        person.zipcode = $scope.person.zipcode;
                        person.fuzzyMatchValue = $scope.person.fuzzyMatchValue;
                        person.isPriority = $scope.person.isPriority;
                        person.dateCreated = moment().format('MM/DD/YYYY');
                        person.isDonor = $scope.person.isDonor;
                        
                        datacontext.save();
                        $modalInstance.close($scope.person);
                        vm.people.unshift(person);
                        logSuccess('Saved changes!', null, true);
                    };

                    $scope.cancel = function () {
                        datacontext.rejectChanges();
                        $modalInstance.dismiss('cancel');
                    }

                },
                size: size,
                resolve: {
                    person: function () {
                        return newPerson;
                    }
                }
            });

            modalInstance.result.then(function (selectedPerson) {
            }, function () {
                log('Modal dismissed at: ' + new Date(), null, false);
            });

        }

        function updatePerson(size, selectedPerson) {

            var modalInstance = $modal.open({
                templateUrl: 'app/people/editPerson.html',
//                windowTemplateUrl: 'app/people/customModal.html', 
                controller: function ($scope, $modalInstance, person, datacontext) {
                    log('person', person, false);
                    $scope.person = {
                        firstname: person.firstname,
                        lastname: person.lastname,
                        emailAddress: person.emailAddress,
                        accountId: person.accountId,
                        zipcode: person.zipcode,
                        isDonor: person.isDonor,
                        isPriority: person.isPriority, 
                        fuzzyMatchValue: person.fuzzyMatchValue
                    }; 

                    $scope.save = function () {
                        person.firstname = $scope.person.firstname;
                        person.lastname = $scope.person.lastname;
                        person.emailAddress = $scope.person.emailAddress;
                        person.accountId = $scope.person.accountId;
                        person.zipcode = $scope.person.zipcode;
                        person.fuzzyMatchValue = $scope.person.fuzzyMatchValue;
                        person.isPriority = $scope.person.isPriority;
                        person.isDonor = $scope.person.isDonor; 

                        datacontext.save();
                        $modalInstance.close($scope.person);
                        logSuccess('Saved changes!', null, true);
                    };

                    $scope.cancel = function () {
                        datacontext.rejectChanges();
                        $modalInstance.dismiss('cancel');
                    }

                },
                size: size,
                resolve: {
                    person: function () {
                        return selectedPerson;
                    }
                }
            });

            modalInstance.result.then(function (selectedPerson) {
            }, function () {
                log('Modal dismissed at: ' + new Date(), null, false);
            });

        }

        function getPeople(forceRefresh) {
            var orderBy = vm.orderByField + (vm.reverseSort ? ' desc' : '');

            datacontext.getPeople(vm.paging.currentPage, vm.paging.pageSize, vm.peopleSearch, vm.fuzzyMatchValue, vm.daysFilter, vm.isPriority, vm.isLocal, orderBy).then(function (data) {
                vm.people = data.results;
                vm.peopleFilteredCount = data.inlineCount;
                //Set count here to limit unnecessary call on first load since filter and total count will be same. 
                if (!vm.peopleSearch) {
                    vm.peopleCount = vm.peopleFilteredCount;
                }

                if (!vm.peopleCount || forceRefresh) {
                    getPeopleCount();
                }
                return vm.people;
            }); 
        }

        function getPeopleCount() {
            return datacontext.getPeopleCount().then(function (data) {
                vm.peopleCount = data;
//                $scope.$apply();
                return vm.peopleCount = data;
            });
        }

        //Do not need because returning count in getpeople function. 
        function getPeopleFilteredCount() {
            datacontext.getPeopleFilteredCount(vm.peopleSearch).then(function (data) {
                $scope.$apply();
                return vm.peopleFilteredCount = data;
            });
        }


        function clearSearch($event) {
            if ($event.keyCode === keyCodes.esc) {
                vm.peopleSearch = '';
                refresh();
            }

        }

        function deletePerson(person) {
            datacontext.markDeleted(person).then(function () {
                datacontext.save();
                logSuccess('Deleted: ' + person.firstname + ' ' + person.lastname, null, true);
                var i = vm.people.indexOf(person);
                vm.people.splice(i, 1);
            });
        }

        function pageChanged(page) {
            getPeople();
        }

        function refresh() {
            getPeople();
        }

        function search($event) {
            getPeople();
        }

        function sort(sortBy) {
            if (sortBy === vm.orderByField) {
                vm.reverseSort = !vm.reverseSort;
            } else {
                vm.orderByField = sortBy;
                vm.reverseSort = false;
            }
            getPeople();
        }

    }
})();