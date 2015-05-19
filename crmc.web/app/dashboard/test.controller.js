(function () {
    "use strict";

    function testCtrl($scope) {
        
/*        $.connection.hub.start().done(function () {
            console.log('connection started');
        })

        var proxy1 = $.connection.crmcHub;

        proxy1.client.addMessage = function (message) {
            console.log('message received', message);
        }

        var crmc1 = $.connection.crmcHub; 

        $scope.sendMessage = function () {
            console.log('message send from test');
            crmc1.server.sendMessage('test');
        }*/
    }

    angular.module('app').controller('TestCtrl', ['$scope', testCtrl])
}());
