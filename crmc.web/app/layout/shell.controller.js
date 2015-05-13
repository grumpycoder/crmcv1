(function () {
    'use strict';

    angular.module('app').controller('ShellCtrl',
                                    ['$location', '$rootScope', '$route', 'config', 'currentUser', 'routes', ShellCtrl]);

    //    ShellCtrl.$inject = ['$rootScope'];

    function ShellCtrl($location, $rootScope, $route, config, currentUser, routes) {
        /* jshint validthis:true */
        var vm = this;

        vm.showSpinner = false;
        vm.spinnerMessage = 'Retrieving data...';

        vm.spinnerOptions = {
            radius: 40,
            lines: 8,
            length: 0,
            width: 30,
            speed: 1.7,
            corners: 1.0,
            trail: 100,
            color: '#428bca'
        };

        vm.isCurrent = isCurrent;
        vm.routes = routes;

        vm.user = {
            loggedIn: false
    };

        vm.login = login; 
        vm.logout = logout;

        activate();

        function activate() {
            getNavRoutes();
            vm.user = currentUser.profile;
            console.log(vm.user.loggedIn);
        }

        function getNavRoutes() {
            vm.navRoutes = routes.filter(function (r) {
                return r.config.settings && r.config.settings.nav;
            }).sort(function (r1, r2) {
                return r1.config.settings.nav > r2.config.settings.nav;
            });
        }

        function isCurrent(route) {
            if (!route.config.title || !$route.current || !$route.current.title) {
                return '';
            }
            var menuName = route.config.title;
            return $route.current.title.substr(0, menuName.length) === menuName ? 'current' : '';
        }

        function login() {
            $location.path('/login');
        }

        function logout() {
            currentUser.profile.username = '';
            currentUser.profile.token = '';
            currentUser.remove();
            $location.path('/login');
            console.log('redirect to login');
        }

        $rootScope.$on('spinner.toggle', function (event, args) {
            vm.showSpinner = args.show;
            if (args.message) {
                vm.spinnerMessage = args.message;
            }
        });

    }
})();
