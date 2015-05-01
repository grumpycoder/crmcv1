﻿
(function () {
    'use strict';

    var controllerId = 'CensorCtrl';
    angular.module('app').controller(controllerId,
        ['$scope', 'appSpinner', 'common', 'config', 'datacontext', censorCtrl]);

    function censorCtrl($scope, appSpinner, common, config, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var logError = getLogFn(controllerId, 'error');
        var logSuccess = getLogFn(controllerId, 'success');
        var keyCodes = config.keyCodes;
        var applyFilter = function () { };

        vm.addItem = addItem;
        vm.cancelEdit = cancelEdit;
        vm.censors = [];
        vm.censorsFilter = censorsFilter;
        vm.censorSearch = ''; 
        vm.currentEdit = {};
        vm.deleteItem = deleteItem;
        vm.editItem = editItem;
        vm.filteredCensors = [];
        vm.orderByField = 'word';
        vm.reverseSort = false;
        vm.saveItem = saveItem;
        vm.search = search;
        vm.sort = sort;
        vm.title = 'Censors';

        activate();

        function activate() {
            common.activateController([getCensorList()], controllerId)
                .then(function() {
                    applyFilter = common.createSearchThrottle(vm, 'censors');
                    if (vm.censorSearch) { applyFilter(true); }
                    log('Activated Censors View');
            });
        }

        //#region Internal Methods        

        function addItem() {
            vm.censor = datacontext.create('Censor', { word: vm.newCensorWord });
            vm.newWordText = '';
            datacontext.save();
            vm.censors.unshift(vm.censor);

        }

        function cancelEdit(id) {
            vm.currentEdit[id] = false;
        }

        function censorsFilter(censor) {
            var textContains = common.textContains;
            var searchText = vm.censorSearch;
            var isMatch = searchText ?
                textContains(censor.word, searchText)
                : true;
            return isMatch;
        }

        function deleteItem(censor) {
            datacontext.markDeleted(censor).then(function () {
                datacontext.save();
                vm.updateCache = true;
                var i = vm.censors.indexOf(censor);
                vm.censors.splice(i, 1);
                logSuccess('Deleted: ' + censor.word, censor);
            });
        }

        function editItem(item) {
            vm.currentEdit[item.id] = true;
        }

        function getCensorList(forceRemote) {
            appSpinner.showSpinner('Retrieving Censors');
            var orderBy = vm.orderByField + (vm.reverseSort ? ' desc' : '');
            datacontext.getCensors(forceRemote, orderBy).then(function(data) {
                appSpinner.hideSpinner();
                return vm.censors = vm.filteredCensors = data;
            })
        }

        function saveItem(item) {
            datacontext.save();
            vm.currentEdit[item.id] = false;
        }

        function search($event) {
            if ($event.keyCode === keyCodes.esc) {
                vm.censorSearch = '';
                applyFilter(true);
            } else {
                applyFilter();
            }
        }

        function sort(sortBy) {
            if (sortBy === vm.orderByField) {
                vm.reverseSort = !vm.reverseSort;
            } else {
                vm.orderByField = sortBy;
                vm.reverseSort = false;
            }
            getCensorList();
        }

        //#endregion
    }
})();
