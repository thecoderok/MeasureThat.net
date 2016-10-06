var appId: string = "benchmarkApp";
var factoryId: string = 'benchmarkListFactory';

(() => {
    'use strict';
    var app = angular.module(appId, []);
    app.factory(factoryId, ['$http', benchmarkListFactory]);

    function benchmarkListFactory($http: ng.IHttpService) {
        function getBenchmarks(): ng.IHttpPromise<any>{
            return $http.get('/api/Benchmarks');
        }

        const service = {
            getBenchmarks: getBenchmarks
        };

        return service;
    }

    // controller
    var BenchmarksController = function (benchmarkListFactory) {

        var vm = this;
        vm.benchmarks = [];

        benchmarkListFactory.getBenchmarks()
            .success(data => {
                this.benchmarks = data;
                console.log('Received benchmarks: ' + data.length);
            })
            .error(error => {
                alert(error);
            });
    }

    // component
    app.component('benchmarkList', {
        templateUrl: '/js/partials/benchmarkList.html',
        controller: BenchmarksController,
        controllerAs: 'vm',
        bindings: {
            benchmarks: '@'
        }
    });
})();

