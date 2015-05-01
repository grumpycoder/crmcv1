(function () {
    "use strict";
    var app = angular.module('app', [
        //Angular Modules
        'ngRoute',
        'ngAnimate',
        'ui.router',
        'ngCookies',
        'ngMessages',
        //Custom Modules
        'common',

        //3rd Party Modules
        'angularSpinner',
        'ui.bootstrap',
        'breeze.angular',
        'breeze.directives'
    ])

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider
            .state('welcome', {
                url: '/welcome',
                templateUrl: 'app/kiosk/welcome.html',
                controller: 'welcome',
                controllerAs: 'vm'
            })
            .state('finish', {
                url: '/finish',
                templateUrl: 'app/kiosk/finish.html'
            })
            .state('find', {
                abstract: true,
                url: '/find',
                templateUrl: 'app/kiosk/find/home.html',
                controller: 'findCtrl',
                controllerAs: 'vm'
            })
               .state('find.search', {
                   url: 'search',
                   templateUrl: 'app/kiosk/find/search.html'
               })
               .state('find.searchresult', {
                   url: 'searchresult',
                   templateUrl: 'app/kiosk/find/searchresult.html'
               })
            .state('create', {
                abstract: true,
                url: '/create',
                templateUrl: 'app/kiosk/create/home.html',
                controller: 'createCtrl',
                controllerAs: 'vm'
            })
               .state('create.inputname', {
                   url: 'inputname',
                   templateUrl: 'app/kiosk/create/inputname.html'
               })
               .state('create.review', {
                   url: 'review',
                   templateUrl: 'app/kiosk/create/review.html'
               });

        $urlRouterProvider.otherwise('/welcome');
    }]);

    app.run(['$route', 'entityManagerFactory', 'common', 'datacontext', function ($route, emFactory, common, datacontext) {
        // Include $route to kick start the router.
        datacontext.loadMetadata();
    }]);
}());
