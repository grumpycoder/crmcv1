(function () {
    'use strict';

    var serviceId = 'entityManagerFactory';
    angular.module('app').factory(serviceId, ['breeze', 'config', 'common', 'model', emFactory]);

    function emFactory(breeze, config, common, model) {
        var $q = common.$q;
        // Convert server-side PascalCase to client-side camelCase property names
        breeze.NamingConvention.camelCase.setAsDefault();
        // Do not validate when we attach a newly created entity to an EntityManager.
        // We could also set this per entityManager
        new breeze.ValidationOptions({ validateOnAttach: false }).setAsDefault();

        var serviceName = config.remoteServiceName;
//        var metadataStore = new breeze.MetadataStore();
        var metadataStore = createMetadataStore();

        var provider = {
            metadataStore: metadataStore,
            newManager: newManager,
            setupMetadata: setupMetadata
        };

        return provider;

        function newManager() {
            var mgr = new breeze.EntityManager({
                serviceName: serviceName,
                metadataStore: metadataStore
            });
            return mgr;
        }

        function createMetadataStore() {
            var store = new breeze.MetadataStore();
            model.configureMetadataStore(store);
            return store;
        }

        function setupMetadata() {
            var deferred = $q.defer();

            metadataStore.fetchMetadata(serviceName).then(function (rawMetadata) {
                // do something with the metadata
                deferred.resolve();
            });
        }

    }
})();

//(function () {
//    'use strict';
//
//    var serviceId = 'entityManagerFactory';
//    angular.module('app').factory(serviceId, ['config', 'model', emFactory]);
//
//    function emFactory(config, model) {
//        breeze.config.initializeAdapterInstance('modelLibrary', 'backingStore', true);
//        breeze.NamingConvention.camelCase.setAsDefault();
//
//        var serviceName = config.remoteServiceName;
//        var metadataStore = createMetadataStore();
//
//        var provider = {
//            metadataStore: metadataStore,
//            newManager: newManager
//        };
//
//        return provider;
//
//        function createMetadataStore() {
//            var store = new breeze.MetadataStore();
//            model.configureMetadataStore(store);
//            return store;
//        }
//
//        function newManager() {
//            var mgr = new breeze.EntityManager({
//                serviceName: serviceName,
//                metadataStore: metadataStore
//            });
//            return mgr;
//        }
//    }
//})();

//(function () {
//    'use strict';
//
//    var serviceId = 'entityManagerFactory';
//    angular.module('app').factory(serviceId, ['config', 'model', emFactory]);
//
//    function emFactory(config, model) {
//        breeze.config.initializeAdapterInstance('modelLibrary', 'backingStore', true);
//        breeze.NamingConvention.camelCase.setAsDefault();
//
//        var serviceName = config.remoteServiceName;
//        var metadataStore = createMetadataStore();
//
//        var provider = {
//            metadataStore: metadataStore,
//            newManager: newManager
//        };
//
//        return provider;
//
//        function createMetadataStore() {
//            var store = new breeze.MetadataStore();
//            model.configureMetadataStore(store);
//            return store;
//        }
//
//        function newManager() {
//            var mgr = new breeze.EntityManager({
//                serviceName: serviceName,
//                metadataStore: metadataStore
//            });
//            return mgr;
//        }
//    }
//})();
