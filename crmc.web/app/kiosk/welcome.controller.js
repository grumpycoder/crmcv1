(function () {


    function welcomeCtrl($state) {
        var vm = this;

        vm.title = 'welecome';
        vm.unlockSettings = unlockSettings;
        var keyCode = '';
        var keyCount = 0;

        function unlockSettings(key) {
            
            if (key === 1) {
                keyCode = key;
            } else {
                keyCode += key.toString();
            }

            if (keyCode === '1234') {
                $state.go('settings');
            }
            console.log(keyCode);
        }

    }

    angular.module('app').controller('welcome', ['$state', welcomeCtrl]);

})();