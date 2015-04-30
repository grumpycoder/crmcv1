(function () {
    'use strict';

    var app = angular.module('app');

    // Collect the routes
    app.constant('routes', getRoutes());
    
    // Configure the routes and route resolvers
    app.config(['$routeProvider', 'routes', routeConfigurator]);
    function routeConfigurator($routeProvider, routes) {

        routes.forEach(function (r) {
            //$routeProvider.when(r.url, r.config);
            setRoute(r.url, r.config);
        });
        $routeProvider.otherwise({ redirectTo: '/' });

        function setRoute(url, definition) {
            // Sets resolvers for all of the routes
            // by extending any existing resolvers (or creating a new one).
            definition.resolve = angular.extend(definition.resolve || {}, {
                prime: prime
            });
            $routeProvider.when(url, definition);
            return $routeProvider;
        }
    }

    prime.$inject = ['datacontext'];
    function prime(dc) { return dc.prime(); }

    // Define the routes 
    function getRoutes() {
        return [
    {
        url: '/',
        config: {
            templateUrl: 'app/dashboard/dashboard.html',
            title: 'dashboard',
            settings: {
                nav: 1,
                content: '<i class="fa fa-dashboard"></i> Dashboard'
            }
        }
    }, {
        url: '/admin',
        config: {
            title: 'admin',
            templateUrl: 'app/admin/admin.html',
            settings: {
                nav: 2,
                content: '<i class="fa fa-lock"></i> Admin'
            }
        }
    }, {
        url: '/censor',
        config: {
            title: 'censor',
            templateUrl: 'app/censor/censor.html',
            settings: {
                nav: 3,
                content: '<i class="fa fa-ban"></i> Censor Words'
            }
        }
    }, {
        url: '/people',
        config: {
            title: 'people',
            templateUrl: 'app/people/people.html',
            settings: {
                nav: 4,
                content: '<i class="fa fa-users"></i> People'
            }
        }
    }, {
        url: '/settings',
        config: {
            title: 'settings',
            templateUrl: 'app/settings/settings.html',
            settings: {
                nav: 5,
                content: '<i class="fa fa-cogs"></i> Settings'
            }
        }
    }
        ];
    }
})();