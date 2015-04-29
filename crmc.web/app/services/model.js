(function () {
    'use strict';

    // Factory name is handy for logging
    var serviceId = 'model';

    // Define the factory on the module.
    // Inject the dependencies. 
    // Point to the factory definition function.
    angular.module('app').factory(serviceId, model);

    function model() {
        // Define the functions and properties to reveal.
        var entityNames = {
            person: 'Person',
            censor: 'Censor',
            appConfig: 'AppConfig'
        };

        var service = {
            configureMetadataStore: configureMetadataStore,
            entityNames: entityNames
        };

        return service;

        function configureMetadataStore(metadataStore) {
            registerPerson(metadataStore);
        }

        //#region Internal Methods        

        function registerPerson(metadataStore) {
            metadataStore.registerEntityTypeCtor('Person', Person);

            function Person() {
                this.isCensored = false;
            }

            Object.defineProperty(Person.prototype, 'fullName', {
                get: function () {
                    var fn = this.firstname;
                    var ln = this.lastname;
                    return ln ? fn + ' ' + ln : fn;
                }
            });
        }

        //#endregion
    }
})();