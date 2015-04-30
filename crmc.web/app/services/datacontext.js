(function () {
    'use strict';

    var serviceId = 'datacontext';
//    angular.module('app').factory(serviceId, ['common', 'config', 'entityManagerFactory', datacontext]);

    angular.module('app').factory(serviceId, ['common', 'entityManagerFactory', 'config', datacontext]);

    function datacontext(common, emFactory, config) {
        var EntityQuery = breeze.EntityQuery;
        var Predicate = breeze.Predicate;
//        var events = config.events;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(serviceId);
        var logError = getLogFn(serviceId, 'error');
        var logInfo = getLogFn(serviceId, 'info');
        var logSuccess = getLogFn(serviceId, 'success');
        var manager = emFactory.newManager();
        var primePromise;
        var $q = common.$q;

        var storeMeta = {
            isLoaded: {
                censors: false,
                people: false
            }
        };

        var service = {
            create: create,
            getAppSettings: getAppSettings,
            getCensors: getCensors,
            getPeople: getPeople,
            getPeopleCount: getPeopleCount,
            getPeopleFilteredCount: getPeopleFilteredCount,
            getMessageCount: getMessageCount,
            markDeleted: markDeleted,
            rejectChanges: rejectChanges,
            prime: prime,
            save: save,
            loadMetadata: loadMetadata
        };

        return service;

        function create(entity, initValues) {
//            manager.fetchMetadata().then(function() {
                var e = manager.createEntity(entity, initValues);
//                log(e);
                return e;
//            })
        }

        function getAppSettings() {
            return EntityQuery.from('AppSettings')
                              .using(manager)
                              .execute()
                              .then(success, _queryFailed);

            function success(data) {
                log('Retreived AppSettings', data.results[0], false);
                return data.results[0];
            }
        }

        function getCensors(forceRemote) {
            var censors;

            if (_areCensorsLoaded() && !forceRemote) {
                censors = EntityQuery.from('Censors')
                                     .using(manager)
                                     .executeLocally();

//                log('Retrieved from local cache', null, false);
                return $q.when(censors);
            }

            return EntityQuery.from('Censors')
                            .using(manager)
                            .toType('Censor')
                            .execute()
                            .then(success, _queryFailed);

            function success(data) {
                censors = data.results;
                _areCensorsLoaded(true);
//                log('Retrieved [Censors] from remote data source', censors.length, false);
                return censors;
            }
        }

        function markDeleted(entity) {
            return $q.when(entity.entityAspect.setDeleted());
        }

        function loadMetadata() {
            if (primePromise) return primePromise;

            primePromise = $q.all([])
              .then(fetchMetadata)
              .then(success);
            return primePromise;

            function success() {
                log('Primed the data', null, false);
            }

            function fetchMetadata() {
                manager.fetchMetadata();
            }

        }

        function prime() {
            if (primePromise) return primePromise;

            primePromise = $q.all([])
                .then(extendMetadata)
                .then(fetchMetadata)
                .then(success);
            return primePromise;

            function success() {
                log('Primed the data', null, false);
            }

            function fetchMetadata() {
                manager.fetchMetadata();
            }

            function extendMetadata() {
                var metadataStore = manager.metadataStore;
            }

        }

        function rejectChanges() {
            manager.rejectChanges();
            logInfo('Canceled changes', null, true);
        }

        function save() {
            manager.saveChanges();
        }

        //TODO: Remove?
        function getMessageCount() { return $q.when(72); }

        function getPeople(page, size, nameFilter, fuzzyMatchFilter, daysFilter, orderBy) {
            orderBy = orderBy || 'dateCreated desc';
            var take = size || 20;
            var skip = page ? (page - 1) * size : 0;
            var predicates = null; 
            var namePredicate = null;
            var fuzzyPredicate = null;
            var dayPredicate = null;

            if (nameFilter) { namePredicate = _fullNamePredicate(nameFilter); }
            if (fuzzyMatchFilter) { fuzzyPredicate = _fuzzyMatchPredicate(fuzzyMatchFilter); }
            if (daysFilter) { dayPredicate = _daysPredicate(daysFilter) }

            predicates = Predicate.and(namePredicate, fuzzyPredicate, dayPredicate);

            return EntityQuery.from('People')
                .take(take)
                .skip(skip)
                .where(predicates)
                .inlineCount()
                .orderBy(orderBy)
                .using(manager)
                .execute()
                .then(success, _queryFailed);

            function success(data) {
                log('Retrieved [People] from remote storage', data.results.length, false);
                return data;
            }
        }

        function getPeopleCount() {
            return EntityQuery.from('People').take(0).inlineCount().using(manager).execute().then(function (data) {
                return data.inlineCount;
            });
        }

        function getPeopleFilteredCount(nameFilter) {
            var predicate = _fullNamePredicate(nameFilter);

            return EntityQuery.from('People').where(predicate).take(0).inlineCount().using(manager).execute().then(function (data) {
                return data.inlineCount;
            });
        }

        function _areCensorsLoaded(value) {
            return _areItemsLoaded('censors', value);
        }

        function _arePeopleLoaded(value) {
            return _areItemsLoaded('people', value);
        }

        function _areItemsLoaded(key, value) {
            if (value === undefined) {
                return storeMeta.isLoaded[key]; // get
            }
            return storeMeta.isLoaded[key] = value; // set
        }

        function _fullNamePredicate(filterValue) {
            var names = filterValue.split(' ');
            var fn = names[1] ? names[0] : '';
            var ln = names[1] || names[0];

            return Predicate
                .create('firstname', 'contains', fn)
                .and('lastname', 'contains', ln);
        }

        function _fuzzyMatchPredicate(fuzzyValue) {
            var value;
            var predicate = null;

            switch (fuzzyValue) {
                case 'high':
                    predicate = Predicate.create('fuzzyMatchValue', '>=', '0.8');
                    break;
                case 'low':
                    predicate = Predicate.create('fuzzyMatchValue', '<', '0.8').and('fuzzyMatchValue', '>=', '0.5');
                    break;
                default:
                    break;
            }
            return predicate;

        }

        function _daysPredicate(filterValue) {
            var dateValue = moment().subtract(parseInt(filterValue), 'days').calendar()
            log('dateValue', dateValue, null);
            return  Predicate.create('dateCreated', '>=', dateValue);
        }

        function _getAllLocal(resource, ordering, predicate) {
            return EntityQuery.from(resource)
                .orderBy(ordering)
                .where(predicate)
                .using(manager)
                .executeLocally();
        }

        function _getInlineCount(data) { return data.inlineCount; }

        function _queryFailed(error) {
            var msg = config.appErrorPrefix + 'Error retrieving data.' + error.message;
            logError(msg, error, null);
            throw error;
        }

    }
})();