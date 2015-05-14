
(function () {
    'use strict';

    angular.module('app', [
            // Angular modules 
            'ngRoute',
            'ngResource',
            'ngAnimate',
            'ui.router',
            'ngCookies',
            'ngMessages',
            //Custom Modules
            'common',

            //3rd Party Modules
            'ui.bootstrap',
            'breeze.angular',
            'breeze.directives',
            'LocalStorageModule',
            'ngTagsInput'
    ])
        .config(['$routeProvider', '$httpProvider', configRoutes]).run(['$route', function ($route) { }])
        .config(['$httpProvider', function ($httpProvider) { $httpProvider.interceptors.push('spinnerInterceptor'); }])

    function configRoutes($routeProvider) {
    }
})();
