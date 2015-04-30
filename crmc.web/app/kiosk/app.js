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
               })
;

        $urlRouterProvider.otherwise('/welcome');
    }]);

    //    app.config(['$stateProvider'],
    //        function () {
    //        
    //    })

    app.run(['$route', 'entityManagerFactory', 'common', 'datacontext', function ($route, emFactory, common, datacontext) {
        // Include $route to kick start the router.
//        breeze.core.extendQ($rootScope, $q);
//        emFactory.metadataStore.fetchMetadata();
//                emFactory.setupMetadata();
//        var $q = common.$q;
//
//        $q.when(emFactory.metadataStore.fetchMetadata());
        datacontext.loadMetadata(); 

    }]);
}());
 


//(function () {
//    'use strict';
//
//    var app = angular.module('app', [
//        // Angular modules 
//        'ngAnimate',
//        'ui.router',
//        'ngCookies',
//        // Custom modules 
////        'common',
//
//        // 3rd Party Modules
////        'ngTable',
//        'ngMessages',
//        'ui.bootstrap',
////        'ui.bootstrap.showErrors',
////        'ui.slider',
////        'angularSpinner',
////        'ngFabForm'
//    ]);
//
//    app.config(['$stateProvider', '$urlRouterProvider', 'usSpinnerConfigProvider', function ($stateProvider, $urlRouterProvider, usSpinnerConfigProvider) {
//
//        $stateProvider
//            .state('welcome', {
//                url: '/welcome',
//                templateUrl: 'app/kiosk/welcome.html',
//                controller: 'welcome',
//                controllerAs: 'vm'
//            })
////            .state('create', {
////                abstract: true,
////                url: '/create',
////                templateUrl: 'app/kiosk/new/create-start.html',
////                controller: 'createname',
////                controllerAs: 'vm'
////            })
////                .state('create.step1', {
////                    url: 'step1',
////                    templateUrl: 'app/kiosk/new/create-step1.html'
////                })
////                .state('create.step2', {
////                    url: 'step2',
////                    templateUrl: 'app/kiosk/new/create-step2.html'
////                })
////                .state('create.finish', {
////                    url: 'finish',
////                    templateUrl: 'app/kiosk/new/finish.html'
////                })
////            .state('search', {
////                abstract: true,
////                url: '/search',
////                templateUrl: 'app/kiosk/search/start.html',
////                controller: 'search',
////                controllerAs: 'vm'
////            })
////                .state('search.startSearch', {
////                    url: 'start',
////                    templateUrl: 'app/kiosk/search/start.html'
////                })
////                .state('search.step1', {
////                    url: 'step1',
////                    templateUrl: 'app/kiosk/search/step1.html'
////                })
////                .state('search.step2', {
////                    url: 'step2',
////                    templateUrl: 'app/kiosk/search/step2.html'
////                })
////                .state('search.finish', {
////                    url: 'finish',
////                    templateUrl: 'app/kiosk/search/finish.html'
////                })
////        .state('settings', {
////            url: 'settings',
////            templateUrl: 'app/kiosk/settings/settings.html',
////            controller: 'settings',
////            controllerAs: 'vm'
////        })
//        ;
//
//        $urlRouterProvider.otherwise('/welcome');
//        
//        usSpinnerConfigProvider.setDefaults({ color: 'black' });
//
//    }]);
//
//    app.run([
//        'model', function (model) {
////            model.configureModel();
//        }
//    ]);
//
//})();
