using Microsoft.Playwright;

namespace E2ETests
{
    [TestClass]
    public class RunBenchmarkTest : BenchmarkLabBaseTest
    {
        const string ASYNC_TEST_URL = "/Benchmarks/Show/32502/2/async-test";
        const string DEFERRED_TEST_URL = "/Benchmarks/Show/32503";
        const string CLASSNAMES_VS_CLXS_VS_CX_URL = "/Benchmarks/Show/32422/0/classnames-vs-clsx-vs-cx";
        const string TYPE_COERCION_BENCHMARK_URL = "/Benchmarks/Show/32429/0/type-coercion-benchmark-2";
        // need globalEval here, there was a bug for it
        const string CLASS_VS_PROTOTYPE_PERFORMANCE_URL = "/Benchmarks/Show/14473/0/class-vs-prototype-performance";
        // need globalEval here, there was a bug for it
        const string IFELSE_VS_SMALL_SWITCH_URL = "/Benchmarks/Show/32454/1/ifelse-vs-small-switch";
        const string PYTHON_PYODIDE_TEST_URL = "/Benchmarks/Show/32635";


        [TestMethod]
        public async Task TestAsyncBenchmark()
        {
            await ValidateBenchmarkCanRun(ASYNC_TEST_URL);
        }

        [TestMethod]
        public async Task TestDeferredBenchmark()
        {
            await ValidateBenchmarkCanRun(DEFERRED_TEST_URL);
        }

        [TestMethod]
        public async Task TestClassnamesVsClxsVsCxBenchmark()
        {
            await ValidateBenchmarkCanRun(CLASSNAMES_VS_CLXS_VS_CX_URL);
        }

        [TestMethod]
        public async Task TestTypeCoercionBenchmark()
        {
            await ValidateBenchmarkCanRun(TYPE_COERCION_BENCHMARK_URL);
        }

        [TestMethod]
        public async Task TestClassVsPrototypePerformanceBenchmark()
        {
            await ValidateBenchmarkCanRun(CLASS_VS_PROTOTYPE_PERFORMANCE_URL);
        }

        [TestMethod]
        public async Task TestIfElseVsSmallSwitchBenchmark()
        {
            await ValidateBenchmarkCanRun(IFELSE_VS_SMALL_SWITCH_URL);
        }

        [TestMethod]
        public async Task TestPythonBenchmark()
        {
            await ValidateBenchmarkCanRun(PYTHON_PYODIDE_TEST_URL, false, false);
        }
    }
}
