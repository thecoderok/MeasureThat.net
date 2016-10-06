var personApp = angular.module('benchmarkApp', []);
personApp.controller('benchmarksController', ['$scope', $scope => {
    $scope.firstName = "Mary";
    $scope.lastName = "Jane";
}]);


// controller
var BenchmarksController = function () {

    var vm = this;
    vm.firstName = "Aftab";
    vm.lastName = "Ansari";
}

// component
personApp.component('benchmarkList', {
    templateUrl: '/js/partials/benchmarkList.html',
    controller: BenchmarksController,
    controllerAs: 'vm'

});
