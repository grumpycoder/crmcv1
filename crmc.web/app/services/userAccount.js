(function () {
    "use strict";

    angular.module('common').factory('userAccount', ['$resource', 'config', userAccount])

    function userAccount($resource, config) {
       return {
           registration: $resource(config.serverPath + '/api/account/register', null, {
               'registerUser': {method: 'POST'}
           }),
           login: $resource(config.serverPath + '/token', null, {
               'loginUser': {
                   method: 'POST',
                   headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                   transformRequest: function (data, headersGetter) {
                       var str = [];
                       for (var d in data)
                           str.push(encodeURIComponent(d) + "=" +
                                               encodeURIComponent(data[d]));
                       return str.join("&");
                   }
               }
           })
       } 
    }

//    function userAccount($resource, common, config) {
//        return $resource(config.serverPath + '/api/account/register', null, {
//            'registerUser': { method: 'POST' },
//            login: $resource(config.serverPath + "/Token", null,
//                   {
//                       'loginUser': {
//                           method: 'POST',
//                           headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
//                           transformRequest: function (data, headersGetter) {
//                               var str = [];
//                               for (var d in data)
//                                   str.push(encodeURIComponent(d) + "=" +
//                                                       encodeURIComponent(data[d]));
//                               return str.join("&");
//                           }
//
//                       }
//                   })
//        })
//    }
}());



//(function () {
//    "use strict";
//    angular.module('app').factory('userAccount', ['$resource', 'common', 'config', userAccount])
//
//    function userAccount($resource, config) {
//        return {
//            registration: $resource(config.serverPath + '/api/account/register', null,
//                    {
//                        'registerUser': { method: 'POST' }
//                    }),
//            login: $resource(config.serverPath + "/Token", null,
//                    {
//                        'loginUser': {
//                            method: 'POST',
//                            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
//                            transformRequest: function (data, headersGetter) {
//                                var str = [];
//                                for (var d in data)
//                                    str.push(encodeURIComponent(d) + "=" +
//                                                        encodeURIComponent(data[d]));
//                                return str.join("&");
//                            }
//
//                        }
//                    })
//        }
//    }
//}());
