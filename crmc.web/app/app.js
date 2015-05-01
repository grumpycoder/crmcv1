
(function () {
    'use strict';

    angular.module('app', [
        // Angular modules 
        'ngRoute',
        'ngAnimate',
        'ui.router',
        'ngCookies',
        'ngMessages',
        //Custom Modules
        'common',

        //3rd Party Modules
        'ui.bootstrap',
        'breeze.angular',
        'breeze.directives'

    ]).config(['$routeProvider', configRoutes]).run(['$route', function($route) {}]);

    function configRoutes($routeProvider) {
        
        $routeProvider
            .when('/', {
                templateUrl: 'app/dashboard/dashboard.html',
                controller: 'DashboardCtrl',
                controllerAs: 'vm'
            })
            .when('/people', {
                templateUrl: 'app/people/people.html',
                controller: 'PeopleCtrl',
                controllerAs: 'vm'
            })
            .when('/censors', {
                templateUrl: 'app/censors/censors.html',
                controller: 'CensorCtrl',
                controllerAs: 'vm'
            });

        $routeProvider.otherwise({ redirectTo: '#/' });

    }
})();
