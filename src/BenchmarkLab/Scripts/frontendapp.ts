/// <reference path="../typings/index.d.ts" />

var appId: string = "benchmarkApp";
var factoryId: string = 'benchmarkListFactory';

(() => {
    'use strict';
    var app = angular.module(appId, ["ui.router"])
        .config(['$urlRouterProvider', '$stateProvider', configRoutes]);
    app.factory(factoryId, ['$http', benchmarkListFactory]);

    function configRoutes($urlRouterProvider: ng.ui.IUrlRouterProvider,
        $stateProvider: ng.ui.IStateProvider) {
        $urlRouterProvider.otherwise('/app');

        /*$stateProvider
            .state('play', {
                url: '/play',
                templateUrl: 'templates/play.html',
                controller: 'PlayController',
                controllerAs: 'vm',
                resolve: {
                    qas: ['PlayService', function (PlayService) {
                        return PlayService.getQas();
                    }]
                }
            })
            .state('howto', {
                url: '/howto',
                templateUrl: 'templates/howto.html',
                controller: 'HowToController',
                controllerAs: 'vm'
            })
            .state('about', {
                url: '/about',
                templateUrl: 'templates/about.html',
                controller: 'AboutController',
                controllerAs: 'vm'
            });*/
    }

    function benchmarkListFactory($http: ng.IHttpService) {
        function getBenchmarks(): ng.IHttpPromise<any>{
            return $http.get('/api/Benchmarks');
        }

        const service = {
            getBenchmarks: getBenchmarks
        };

        return service;
    }
})();

