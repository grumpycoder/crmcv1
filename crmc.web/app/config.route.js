(function () {
    'use strict';

    var app = angular.module('app');

    // Collect the routes
    app.constant('routes', getRoutes());

    // Configure the routes and route resolvers
    app.config(['$routeProvider', '$sceProvider', 'routes', routeConfigurator]);
    function routeConfigurator($routeProvider, $sceProvider, routes) {
        $sceProvider.enabled(false);

        routes.forEach(function (r) {
            setRoute(r.url, r.config);
        });
        $routeProvider.otherwise({ redirectTo: '/' });

        function setRoute(url, definition) {
            // Sets resolvers for all of the routes
            // by extending any existing resolvers (or creating a new one).
            definition.resolve = angular.extend(definition.resolve || {}, {
                checkSecurity: checkSecurity,
                prime: prime
            });
            $routeProvider.when(url, definition);
            return $routeProvider;
        }
    }

    checkSecurity.$inject = ['$location', '$route', 'common', 'currentUser']
    function checkSecurity($location, $route, common, currentUser) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn('checkSecurity');
        var settings = $route.current.settings;
        var loginRequired = settings.loginRequired || false;
        var roles = settings.roles || [];
        var user = currentUser.profile;

        if (loginRequired) {
            if (!user.loggedIn) {
                $location.path('/login');
            } else {
                if (roles.length > 0) {
                 if (!common.checkRole(user.roles, roles)) {
                     $location.path('/notauthorized').replace();
                 }   
                }
            }
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
                    controller: 'DashboardCtrl',
                    controllerAs: 'vm', 
                    settings: {
                        nav: 1,
                        loginRequired: true, 
                        roles: ['user'],
                        content: '<i class="fa fa-dashboard"></i> Dashboard'
                    }
                }
            }, {
                url: '/censor',
                config: {
                    title: 'censor',
                    templateUrl: 'app/censors/censors.html',
                    controller: 'CensorCtrl',
                    controllerAs: 'vm',
                    settings: {
                        nav: 3,
                        loginRequired: true,
                        roles: ['user'],
                        content: '<i class="fa fa-ban"></i> Censor Words'
                    }
                }
            }, {
                url: '/people',
                config: {
                    title: 'people',
                    templateUrl: 'app/people/people.html',
                    controller: 'PeopleCtrl',
                    controllerAs: 'vm',
                    settings: {
                        nav: 4,
                        loginRequired: true,
                        roles: ['user'],
                        content: '<i class="fa fa-users"></i> People'
                    }
                }
            }, {
                url: '/settings',
                config: {
                    title: 'settings',
                    templateUrl: 'app/settings/settings.html',
                    controller: 'SettingCtrl',
                    controllerAs: 'vm',
                    settings: {
                        nav: 5,
                        loginRequired: true,
                        roles: ['admin'],
                        content: '<i class="fa fa-cogs"></i> Settings'
                    }
                }
            }, {
                url: '/users',
                config: {
                    title: 'users',
                    templateUrl: 'app/users/users.html',
                    controller: 'UserCtrl',
                    controllerAs: 'vm',
                    settings: {
                        nav: 5,
                        loginRequired: true,
                        roles: ['admin'],
                        content: '<i class="fa fa-user-times"></i> Users'
                    }
                }
            }, {
                url: '/login',
                config: {
                    title: 'login',
                    templateUrl: 'app/users/login.html',
                    controller: 'LoginCtrl',
                    controllerAs: 'vm',
                    settings: {
                        loginRequired: false,
                        roles: []
                    }
                }
            }, {
                url: '/notauthorized',
                config: {
                    title: 'notauthorized',
                    templateUrl: 'app/users/notauthorized.html',
                    settings: {
                        loginRequired: false,
                        roles: []
                    }
                }
            }
            , {
                url: '/test',
                config: {
                    title: 'test',
                    templateUrl: 'app/dashboard/test.html',
                    controller: 'TestCtrl',
                    settings: {
                        loginRequired: false,
                        roles: []
                    }
                }
            }
        ];
    }
})();