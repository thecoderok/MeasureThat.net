/// <reference path="../typings/globals/jquery/index.d.ts" />
/// <reference path="../typings/globals/codemirror/index.d.ts" />
/// <reference path="../typings/globals/mustache/index.d.ts" />
/// <reference path="../typings/globals/google.visualization/index.d.ts" />
/// <reference path="../typings/globals/benchmark/index.d.ts" />
/// <reference path="../typings/globals/bootstrap/index.d.ts" />


declare namespace Benchmark {
    export interface Options {
        async?: boolean | undefined;
        defer?: boolean | undefined;
        delay?: number | undefined;
        id?: string | undefined;
        initCount?: number | undefined;
        maxTime?: number | undefined;
        minSamples?: number | undefined;
        minTime?: number | undefined;
        name?: string | undefined;
        onAbort?: Function | undefined;
        onComplete?: Function | undefined;
        onCycle?: Function | undefined;
        onError?: Function | undefined;
        onReset?: Function | undefined;
        onStart?: Function | undefined;
        setup?: Function | string | undefined;
        teardown?: Function | string | undefined;
        fn?: Function | string | undefined;
        queued?: boolean | undefined;
    }
    class Suite {
        add(name: string, options?: Options): Suite;
        run(options?: Benchmark.Options);
        on(type?: string, callback?: Function): Suite;
    }
}

interface IBenchmarkSuiteEventHandler {
    onStartHandler(): void;
    onCycleHandler(event: Event): void;
    onAbortHandler(evt: any): void;
    onErrorHandler(event: { target: { error: string; } }): void;
    onResetHandler(evt: any): void;
    onCompleteHandler(event: Event): void;
}

class BenchmarkSuiteEventHandlerImpl implements IBenchmarkSuiteEventHandler {
    private outerRunner: ClientValidationHandler;
    private testRunner: TestRunnerController;

    constructor(outerRunner: any, testRunner: TestRunnerController) {
        this.outerRunner = outerRunner;
        this.testRunner = testRunner;
    }

    onStartHandler(): void {
        this.testRunner.appendToLog('Started benchmark...');
    }

    onCycleHandler(event: { target: any }): void {
        console.log(String(event.target));
        this.testRunner.appendToLog('Checked test: ' + String(event.target));
    }

    onAbortHandler(evt: any): void {
        this.testRunner.appendToLog('Benchmark aborted');
    }

    onErrorHandler(evt: { target: { error: string; } }): void {
        let message = "Some error occurred.";
        if (evt && evt.target && evt.target.error) {
            message = evt.target.error;
        }
        this.outerRunner.validationFailed(message);
    }

    onResetHandler(evt: any): void {
        this.testRunner.appendToLog('Benchmark reset.');
    }

    onCompleteHandler(suites: Event): void {
        var benchmark = suites.currentTarget as any;
        if ((suites.target as any).aborted === true) {
            return;
        }
        this.outerRunner.validationSuccess();
    }
}


function getElementByDataAttribute(attr: string): HTMLElement {
    const row = window.parent.document.querySelectorAll(attr);
    if (row.length !== 1) {
        throw "Unable to find signle element by attribute: " + attr;
    }
    return row[0] as HTMLElement;
}

function replaceAll(target: string, search: string, replacement: string): string {
    return target.split(search).join(replacement);
}
function getElementsByDataAttribute(attr: string): NodeListOf<Element> {
    const result = window.parent.document.querySelectorAll(attr);
    if (result.length === 0) {
        throw "Unable to find any elements by attribute: " + attr;
    }
    return result;
}

// jQuery's globalEval function. Needed for script preparation code since
// sometimes variables end up with block scope (e.g. consts). 
function globalEval(code: string): void {
    const script = document.createElement('script');
    script.text = code;
    document.head.appendChild(script).parentNode?.removeChild(script);
}

class TestCase {
    constructor(
        public Name: string,
        public Code: string,
        public IsDeferred: boolean
    ) {}
}

class MeasureThatBenchmark {
    constructor(
        public ScriptPreparationCode: string,
        public IsPython: boolean,
        public TestCases: TestCase[]
    ) {}
}

class TestSuiteBuilder {
    constructor(
        private benchmark: MeasureThatBenchmark,
        private eventsHandler: IBenchmarkSuiteEventHandler) {
    }

    public buildSuite(): Benchmark.Suite {
        globalEval(this.benchmark.ScriptPreparationCode);
        var suite: Benchmark.Suite = new Benchmark.Suite();
        for (var i = 0; i < this.benchmark.TestCases.length; i++) {
            var testBody = this.benchmark.TestCases[i].Code;
            var deferred = this.benchmark.TestCases[i].IsDeferred;
            var fn: Function;
            if (deferred) {
                eval("fn = async function (deferred) {" + testBody + "; }");
            } else {
                eval("fn = function () {" + testBody + "; }");
            }
            var options = { 'fn': fn, 'defer': deferred };
            suite.add(this.benchmark.TestCases[i].Name, options);
        }
        suite
            .on('cycle', this.eventsHandler.onCycleHandler)
            .on('complete', this.eventsHandler.onCompleteHandler)
            .on('abort', this.eventsHandler.onAbortHandler)
            .on('error', this.eventsHandler.onErrorHandler)
            .on('reset', this.eventsHandler.onResetHandler)
            .on('start', this.eventsHandler.onStartHandler);
        return suite;
    }
}

class TestRunnerController  implements IBenchmarkSuiteEventHandler {
    private memInfo = Array();
    private shouldRecordMemory = false;
    private invervalId: number = -1;
    
    constructor() {
        document.addEventListener("DOMContentLoaded", () => this.initialize());
        (window as any)._test_runner = this;
    }

    private initialize(): void {
        window.addEventListener("message", (e) => this.handleMessage(e));
        var url = window.location.href;
        if (url.indexOf('?autostart=1') != -1) {
            this.autostartTest();
        }
        if (url.indexOf('?autorefresh=1') != -1) {
            this.autorefreshPage();
        }
    }

    private autorefreshPage(): void {
        // To properly test the HTML Preparation code we need to reload the iframe again.
        this.appendToLog('Taking care of HTML Preparation code...');
        var htmlPrep: string = (parent.window.document.getElementById('HtmlPreparationCode') as HTMLTextAreaElement).value;
        (document.getElementById('htmlPrepCode') as HTMLInputElement).value = htmlPrep;
        (document.getElementById('autoreload_form') as HTMLFormElement).submit();
    }

    private parseBenchmark(): MeasureThatBenchmark {
        const scriptPreparationCode = (parent.document.getElementById('ScriptPreparationCode') as HTMLTextAreaElement).value;
        const isPython = (parent.document.getElementById('IsPython') as HTMLInputElement).checked;
        const testCases = Array.from(parent.document.getElementById('test-case-list').querySelectorAll('[data-role="testCaseComponent"]')).map((tc) => {
            const name = (tc.querySelectorAll('[data-role="testCaseName"]')[0] as HTMLInputElement).value;
            const code = (tc.querySelectorAll('[data-role="testCaseCode"]')[0] as HTMLTextAreaElement).value;
            const isDeferred = (tc.querySelectorAll('[data-role="Deferred"]')[0] as HTMLInputElement).checked;
            return new TestCase(name, code, isDeferred);
        });
        return new MeasureThatBenchmark(scriptPreparationCode, isPython, testCases);
    }

    private parseBenchmarkFromJSON(): MeasureThatBenchmark {
        const jsonString = (document.getElementById("benchmark_definition_json") as HTMLTextAreaElement).value;
        return JSON.parse(jsonString) as MeasureThatBenchmark;
    }

    private autostartTest(): void {
        const outerRunner: ClientValidationHandler = (parent.window as any)._validation_handler;
        this.appendToLog('Loading iframe for testing...Done.');
        this.appendToLog('Attempting to run benchmark...');
        const _myThis = this;
        try {
            // TODO: why do we need 2 ways of building a test suite?
            const benchmark = this.parseBenchmark();
            const benchmarkSuiteBuilder = new TestSuiteBuilder(benchmark, new BenchmarkSuiteEventHandlerImpl(outerRunner, _myThis));
            const suite = benchmarkSuiteBuilder.buildSuite();
            _myThis.appendToLog('Starting benchmark...');
            suite.run({ 'async': true });
        } catch (e) {
            outerRunner.validationFailed(e.message);
        }
    }

    public appendToLog(message: string): void {
        var el = document.createElement('li');
        el.textContent = message;
        window.parent.document.getElementById('validation_log').appendChild(el);
    }

    handleMessage(event: any): void {
        const allowedOrigins = new Set([
            "http://localhost:5000",
            "https://measurethat.net/",
            "https://measurethat.net",
            "https://www.measurethat.net",
            "http://measurethat.net",
            "http://www.measurethat.net",
            "https://benchmarklab.azurewebsites.net"
        ]);

        if (!allowedOrigins.has(event.origin)) {
            // Where did this message come from?
            console.warn('Message from unknown origin: ' + event.origin);
        }

        if (event.data === 'start_test') {
            this.shouldRecordMemory = false;
            this.runTests();
        }

        if (event.data === 'start_test_with_memory_recordings') {
            this.shouldRecordMemory = true;
            this.runTests();
            this.invervalId = setInterval(() => this.recordMemoryInfo(""), 900);
        }
    }

    runTests(): void {
        // TODO: Group these in funciton to enable-disable elements
        window.parent.document.getElementById('runTest').setAttribute('disabled', 'true');
        window.parent.document.getElementById('runTestWithMemory').setAttribute('disabled', 'true');
        window.parent.document.getElementById('spinner').style.display = 'inline-block';
        // Clean up any previous status
        var labels = getElementsByDataAttribute('[data-role="result-label"]');
        for (var i = 0; i < labels.length; ++i) {
            labels[i].textContent = '';
        }
        getElementByDataAttribute('[data-role="fastest-label"]').textContent = '';
        getElementByDataAttribute('[data-role="slowest-label"]').textContent = '';
        window.parent.document.getElementById('results-placeholder').innerHTML = '';
        window.parent.document.getElementById('results-placeholder').style.display = 'none';

        window.parent.document.getElementById('chart_div').innerHTML = '';
        window.parent.document.getElementById('memory_chart_div').innerHTML = '';

        const benchmarkDefinition: MeasureThatBenchmark = this.parseBenchmarkFromJSON();
        const benchmarkSuiteBuilder = new TestSuiteBuilder(benchmarkDefinition, this);
        
        try {
            const suite = benchmarkSuiteBuilder.buildSuite();
            suite.run({ 'async': true });
        } catch (e) {
            alert("Error:" + e.message);
            const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
            suiteStatusLabels.textContent = 'Error';
            suiteStatusLabels.setAttribute("class", "label label-warning");
            window.parent.document.getElementById('runTest').removeAttribute('disabled');
            window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
            window.parent.document.getElementById('spinner').style.display = 'none';
        }
    }

    onStartHandler(): void {
        (window as any)._test_runner.recordMemoryInfo("start");
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        suiteStatusLabels.textContent = 'Running';
        suiteStatusLabels.setAttribute("class", "label label-info");
    }

    onCycleHandler(targets: Event): void {
        (window as any)._test_runner.recordMemoryInfo("test end");
        const completedTarget = targets.target as any;
        var testName: string = completedTarget.name;
        testName = replaceAll(testName, "'", "\\'");
        const row = window.parent.document.querySelectorAll(`[data-row-for='${testName}']`);
        if (row.length !== 1) {
            throw "Unable to find where to report result";
        }
        row[0].querySelectorAll('[data-role="result-label"]')[0].textContent = completedTarget.toString();
    }

    onAbortHandler(): void {
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        suiteStatusLabels.textContent = 'Aborted';
        suiteStatusLabels.setAttribute("class", "label label-warning");
        window.parent.document.getElementById('runTest').removeAttribute('disabled');
        window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
        window.parent.document.getElementById('spinner').style.display = 'inline-block';
    }

    onErrorHandler(evt: { target: { error: string; }; }): void {
        let message = "Some error occurred.";
        if (evt && evt.target && evt.target.error) {
            message = evt.target.error;
        }
        alert(message);
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        suiteStatusLabels.textContent = 'Error';
        suiteStatusLabels.setAttribute("class", "label label-danger");
        window.parent.document.getElementById('runTest').removeAttribute('disabled');
        window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
        window.parent.document.getElementById('spinner').style.display = 'none';
    }

    onResetHandler(): void {
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        suiteStatusLabels.textContent = 'Reset';
        suiteStatusLabels.setAttribute("class", "label label-warning");
        window.parent.document.getElementById('runTest').removeAttribute('disabled');
        window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
        window.parent.document.getElementById('spinner').style.display = 'none';
    }

    onCompleteHandler(suites: Event): void {
        clearInterval((window as any)._test_runner.intervalId);
        (window as any)._test_runner.recordMemoryInfo("complete");
        // typings for benchmark.js are bad  and not complete
        var benchmark = suites.currentTarget as any;
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        if ((suites.target as any).aborted === true) {
            suiteStatusLabels.textContent = 'Aborted';
            suiteStatusLabels.setAttribute("class", "label label-warning");
            return;
        }
        suiteStatusLabels.textContent = 'Completed';
        suiteStatusLabels.setAttribute("class", "label label-success");
        getElementByDataAttribute("[data-role='fastest-label']").textContent = benchmark.filter("fastest").map("name");
        getElementByDataAttribute("[data-role='slowest-label']").textContent = benchmark.filter("slowest").map("name");

        window.parent.document.getElementById('runTest').removeAttribute('disabled');
        window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
        window.parent.document.getElementById('spinner').style.display = 'none';

        (window.parent as any)._benchmark_listener.handleRunCompleted(suites, (window as any)._test_runner.memInfo);
    }

    private recordMemoryInfo(sampleName: string): void {
        if (!this.shouldRecordMemory) {
            return;
        }
        if ((window.performance as any).memory) {
            this.memInfo.push({ name: sampleName, mem: (window.performance as any).memory });
        }
    } 
}