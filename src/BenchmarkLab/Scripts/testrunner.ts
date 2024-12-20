﻿/// <reference path="../typings/globals/jquery/index.d.ts" />
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

declare function loadPyodide(): Promise<any>;
declare function globalMeasureThatScriptPrepareFunction(): Promise<void>;

interface IBenchmarkSuiteEventHandler {
    onStartHandler(): void;
    onCycleHandler(event: Event): void;
    onAbortHandler(evt: any): void;
    onErrorHandler(event: { target: { error: string; } }): void;
    onResetHandler(evt: any): void;
    onCompleteHandler(event: Event): void;
    onPrepareEnvironmentHandler(): void;
}

class BenchmarkSuiteEventHandlerImpl implements IBenchmarkSuiteEventHandler {
    private outerRunner: ClientValidationHandler;
    private testRunner: TestRunnerController;

    constructor(outerRunner: any, testRunner: TestRunnerController) {
        this.outerRunner = outerRunner;
        this.testRunner = testRunner;
    }
    onPrepareEnvironmentHandler(): void {
        this.testRunner.appendToLog('Preparing the environment...');
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
        public TestCases: TestCase[]
    ) {}
}

class TestSuiteBuilder {
    constructor(
        private benchmark: MeasureThatBenchmark,
        private eventsHandler: IBenchmarkSuiteEventHandler) {
    }

    public async buildSuite(): Promise<Benchmark.Suite> {
        this.eventsHandler.onPrepareEnvironmentHandler();
        globalEval(this.benchmark.ScriptPreparationCode);

        if (typeof globalMeasureThatScriptPrepareFunction === 'function') {
            console.log("Found globalMeasureThatScriptPrepareFunction, executing it");
            await globalMeasureThatScriptPrepareFunction();
        }        
        
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
            .on('cycle', (event: Event) => this.eventsHandler.onCycleHandler(event))
            .on('complete', (event: Event) => this.eventsHandler.onCompleteHandler(event))
            .on('abort', (evt: any) => this.eventsHandler.onAbortHandler(evt))
            .on('error', (event: { target: { error: string; } }) => this.eventsHandler.onErrorHandler(event))
            .on('reset', (evt: any) => this.eventsHandler.onResetHandler(evt))
            .on('start', () => this.eventsHandler.onStartHandler());
        return suite;
    }
}

class TestRunnerController  implements IBenchmarkSuiteEventHandler {
    private memInfo = Array();
    private shouldRecordMemory = false;
    private invervalId: number = -1;
    
    constructor() {
        document.addEventListener("DOMContentLoaded",async () => await this.initialize());
        (window as any)._test_runner = this;
    }
    onPrepareEnvironmentHandler(): void {
        this.updateSuiteStatus('Preparing the environment', 'label-info');
    }

    private async initialize(): Promise<void> {
        window.addEventListener("message", async (e) => await this.handleMessage(e));
        var url = window.location.href;
        if (url.indexOf('?autostart=1') != -1) {
            await this.autostartTest();
        }
        if (url.indexOf('?autorefresh=1') != -1) {
            await this.autorefreshPage();
        }
    }

    private async autorefreshPage(): Promise<void> {
        // To properly test the HTML Preparation code we need to reload the iframe again.
        this.appendToLog('Taking care of HTML Preparation code...');
        var htmlPrep: string = (parent.window.document.getElementById('HtmlPreparationCode') as HTMLTextAreaElement).value;
        (document.getElementById('htmlPrepCode') as HTMLInputElement).value = htmlPrep;
        (document.getElementById('autoreload_form') as HTMLFormElement).submit();
    }

    private parseBenchmark(): MeasureThatBenchmark {
        const scriptPreparationCode = (parent.document.getElementById('ScriptPreparationCode') as HTMLTextAreaElement).value;
        const testCases = Array.from(parent.document.getElementById('test-case-list').querySelectorAll('[data-role="testCaseComponent"]')).map((tc) => {
            const name = (tc.querySelectorAll('[data-role="testCaseName"]')[0] as HTMLInputElement).value;
            const code = (tc.querySelectorAll('[data-role="testCaseCode"]')[0] as HTMLTextAreaElement).value;
            const isDeferred = (tc.querySelectorAll('[data-role="Deferred"]')[0] as HTMLInputElement).checked;
            return new TestCase(name, code, isDeferred);
        });
        return new MeasureThatBenchmark(scriptPreparationCode, testCases);
    }

    private parseBenchmarkFromJSON(): MeasureThatBenchmark {
        const jsonString = (document.getElementById("benchmark_definition_json") as HTMLTextAreaElement).value;
        return JSON.parse(jsonString) as MeasureThatBenchmark;
    }

    private async autostartTest(): Promise<void> {
        const outerRunner: ClientValidationHandler = (parent.window as any)._validation_handler;
        this.appendToLog('Loading iframe for testing...Done.');
        this.appendToLog('Attempting to run benchmark...');
        const _myThis = this;
        try {
            const benchmark = this.parseBenchmark();
            const benchmarkSuiteBuilder = new TestSuiteBuilder(benchmark, new BenchmarkSuiteEventHandlerImpl(outerRunner, _myThis));
            const suite = await benchmarkSuiteBuilder.buildSuite();
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

    async handleMessage(event: any): Promise<void> {
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
            await this.runTests();
        }

        if (event.data === 'start_test_with_memory_recordings') {
            this.shouldRecordMemory = true;
            await this.runTests();
            this.invervalId = setInterval(() => this.recordMemoryInfo(""), 900);
        }
    }

    async runTests(): Promise<void> {
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
            const suite = await benchmarkSuiteBuilder.buildSuite();
            suite.run({ 'async': true });
        } catch (e) {
            alert("Error:" + e.message);
            this.updateSuiteStatus('Error', 'label-warning');
            window.parent.document.getElementById('runTest').removeAttribute('disabled');
            window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
            window.parent.document.getElementById('spinner').style.display = 'none';
        }
    }

    onStartHandler(): void {
        (window as any)._test_runner.recordMemoryInfo("start");
        this.updateSuiteStatus('Running', 'label-info');
    }

    private updateSuiteStatus(status: string, label: string): void {
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        suiteStatusLabels.textContent = status;
        suiteStatusLabels.setAttribute("class", `label ${label}`);
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
        this.updateSuiteStatus('Aborted', 'label-warning');
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
        this.updateSuiteStatus('Error', 'label-danger');
        window.parent.document.getElementById('runTest').removeAttribute('disabled');
        window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
        window.parent.document.getElementById('spinner').style.display = 'none';
    }

    onResetHandler(): void {
        this.updateSuiteStatus('Reset', 'label-warning');
        window.parent.document.getElementById('runTest').removeAttribute('disabled');
        window.parent.document.getElementById('runTestWithMemory').removeAttribute('disabled');
        window.parent.document.getElementById('spinner').style.display = 'none';
    }

    onCompleteHandler(suites: Event): void {
        clearInterval((window as any)._test_runner.intervalId);
        (window as any)._test_runner.recordMemoryInfo("complete");
        // typings for benchmark.js are bad  and not complete
        var benchmark = suites.currentTarget as any;
        if ((suites.target as any).aborted === true) {
            this.updateSuiteStatus('Aborted', 'label-warning');
            return;
        }
        this.updateSuiteStatus('Completed', 'label-success');
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