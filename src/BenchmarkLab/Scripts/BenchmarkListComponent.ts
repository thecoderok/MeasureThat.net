/// <reference path="../typings/index.d.ts" />

import {BenchmarkController} from "./BenchmarksController"

// component test
angular.module('benchmarkApp').component('benchmarkList', {
    templateUrl: '/js/partials/benchmarkList.html',
    controller: BenchmarkController,
    controllerAs: 'vm',
    bindings: {
        benchmarks: '@'
    }
});
