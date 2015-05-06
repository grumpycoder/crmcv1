(function () {
    'use strict';

    angular.module('app').controller('ShellCtrl',
                                    ['$rootScope', '$route', 'config', 'routes', 'currentUser', 'userAccount', ShellCtrl]);

    //    ShellCtrl.$inject = ['$rootScope'];

    function ShellCtrl($rootScope, $route, config, routes, currentUser, userAccount) {
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


        vm.isLoggedIn = function (){ return currentUser.getProfile().isLoggedIn;}
        vm.message = '';
        vm.userData = {
            userName: 'abc@abc.com',
            email: 'abc@abc.com',
            password: '!1Password',
            confirmPassword: ''
        };

        vm.registerUser = function () {
            vm.userData.confirmPassword = vm.userData.password;

            userAccount.registration.registerUser(vm.userData,
                function (data) {
                    vm.confirmPassword = "";
                    vm.message = "... Registration successful";
                    vm.login();
                },
                function (response) {
                    vm.isLoggedIn = false;
                    vm.message = response.statusText + "\r\n";
                    if (response.data.exceptionMessage)
                        vm.message += response.data.exceptionMessage;

                    // Validation errors
                    if (response.data.modelState) {
                        for (var key in response.data.modelState) {
                            vm.message += response.data.modelState[key] + "\r\n";
                        }
                    }
                });
        }

        vm.login = function () {
            vm.userData.grant_type = "password";
            vm.userData.userName = vm.userData.email;

            userAccount.login.loginUser(vm.userData,
                function (data) {
                    vm.message = "";
                    vm.password = "";
                    currentUser.setProfile(vm.userData.userName, data.access_token);
               },
                function (response) {
                    vm.password = "";
                    vm.message = response.statusText + "\r\n";
                    if (response.data.exceptionMessage)
                        vm.message += response.data.exceptionMessage;

                    if (response.data.error) {
                        vm.message += response.data.error;
                    }
                });
        }

        activate();

        function activate() {
            getNavRoutes();
            console.log(config.serverPath + '/api/account/register');
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

        $rootScope.$on('spinner.toggle', function (event, args) {
            vm.showSpinner = args.show;
            if (args.message) {
                vm.spinnerMessage = args.message;
            }
        });
    }
})();
