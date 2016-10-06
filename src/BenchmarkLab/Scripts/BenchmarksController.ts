// controller
export function BenchmarkController (benchmarkListFactory) {

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
