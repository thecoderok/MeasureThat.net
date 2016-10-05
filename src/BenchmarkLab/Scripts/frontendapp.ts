var personApp = angular.module('benchmarkApp', []);
personApp.controller('benchmarksController', ['$scope', $scope => {
    $scope.benchmarks = ['Bla1', 'Blah2'];
}]);
