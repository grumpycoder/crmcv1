// blacklist.directive.js
// mark.lawrence

(function() {
    'use strict';

    angular.module('app').directive('blacklistName', function() {
        return {
            restrict: 'A', 
            scope: {
                blacklist: '='
            },
            require: 'ngModel',
            link: function (scope, ele, attrs, ctrl) {
                ctrl.$validators.blacklist = function test(modelValue, viewValue) {
                    var value = modelValue || viewValue;
                    //console.log('from directive: value: ' + value);
                    //console.log('from directive: indexOf: ' + scope.blacklist.indexOf(value.toUpperCase()));
                    if(value) return scope.blacklist.indexOf(value.toUpperCase()) == -1;
                }
            }
        }
    })


})();