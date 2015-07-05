(function () {


    function welcomeCtrl($state) {
        var vm = this;

        vm.title = 'welecome';
        vm.unlockSettings = unlockSettings;
        var keyCode = '';
        var keyCount = 0;

        function unlockSettings(key) {
            keyCode = keyCode + key.toString();
            keyCount += 1;
            if (keyCount === 4) {
                $state.go('settings');
            }

            //if (keyCode === '1234' && keyCode.length === 4) {
            //    console.log('let me in');
            //    $state.go('settings');
            //}

            //if (keyCode.length >= 4 && keyCode !== '1234') {
            //    console.log('invalid code. resetting');
            //    keyCode = key;
            //}
            console.log(keyCode);
        }

    }

    angular.module('app').controller('welcome', ['$state', welcomeCtrl]);

})();