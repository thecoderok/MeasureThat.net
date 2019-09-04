/// <reference path="../typings/globals/jquery/index.d.ts" />
/// <reference path="../typings/globals/codemirror/index.d.ts" />
/// <reference path="../typings/globals/mustache/index.d.ts" />
/// <reference path="../typings/globals/google.visualization/index.d.ts" />
/// <reference path="../typings/globals/benchmark/index.d.ts" />
/// <reference path="../typings/globals/bootstrap/index.d.ts" />

function getElementByDataAttribute(attr: string): HTMLElement {
    const row = window.parent.document.querySelectorAll(attr);
    if (row.length !== 1) {
        throw "Unable to find signle element by attribute: " + attr;
    }
    return row[0] as HTMLElement;
}

function getElementsByDataAttribute(attr: string): NodeListOf<Element> {
    const result = window.parent.document.querySelectorAll(attr);
    if (result.length === 0) {
        throw "Unable to find any elements by attribute: " + attr;
    }
    return result;
}

class TestRunnerController {
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

    private autostartTest(): void {
        var outerRunner: any = (parent.window as any)._validation_handler;
        this.appendToLog('Loading iframe for testing...Done.');
        this.appendToLog('Attempting to run benchmark...');
        var _myThis = this;
        try {
            var htmlPrep: string = (parent.window.document.getElementById('HtmlPreparationCode') as HTMLTextAreaElement).value;
            //window.parent["$"](htmlPrep).appendTo(document.body);
            /*document.getElementById('validation-html-preparation').innerHTML =
                (parent.window.document.getElementById('HtmlPreparationCode') as HTMLTextAreaElement).value;*/
            eval((parent.document.getElementById('ScriptPreparationCode') as HTMLTextAreaElement).value);

            var tc = window.parent.document.getElementById('test-case-list').querySelectorAll('[data-role="testCaseComponent"]');
            var suite:any = eval("new Benchmark.Suite");
            for (var i = 0; i < tc.length; i++) {
                var testName = (tc[i].querySelectorAll('[data-role="testCaseName"]')[0] as HTMLInputElement).value;
                var testBody = (tc[i].querySelectorAll('[data-role="testCaseCode"]')[0] as HTMLTextAreaElement).value;

                eval("suite.add(testName, function () { " + testBody + " })");
            }
            suite.on('cycle', function (event) {
                console.log(String(event.target));
                _myThis.appendToLog('Checked test: ' + String(event.target));
            })
            .on('complete', function (suites: Event) {
                var benchmark = suites.currentTarget as any;
                if ((suites.target as any).aborted === true) {
                    return;
                }
                outerRunner.validationSuccess();
                
            })
            .on('abort', function (evt) {
                console.log('abort: ' + JSON.stringify(evt));
                _myThis.appendToLog('Benchmark abort');
            })
            .on('error', function (evt) {
                let message = "Some error occurred.";
                if (evt && evt.target && evt.target.error) {
                    message = evt.target.error;
                }
                
                outerRunner.validationFailed(message);
            })
            .on('reset', function (evt) {
                _myThis.appendToLog('Benchmark reset.');
                });
            _myThis.appendToLog('Starting benchmark...');
            suite.run({ 'async': true });
        } catch (e) {
            outerRunner.validationFailed(e.message);
        }
    }

    private appendToLog(message: string): void {
        var el = document.createElement('li');
        el.textContent = message;
        window.parent.document.getElementById('validation_log').appendChild(el);
    }

    handleMessage(event: any): void {
        if (event.origin !== "http://localhost:5000"
            && event.origin !== "https://measurethat.net/"
            && event.origin !== "https://measurethat.net"
            && event.origin !== "https://www.measurethat.net"
            && event.origin !== "http://measurethat.net"
            && event.origin !== "http://www.measurethat.net"
            && event.origin !== "https://benchmarklab.azurewebsites.net") {
            // Where did this message came from?
            console.warn('Message from uknown origin: '+ event.origin);
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

        var preparation = document.getElementById("jspreparation").innerHTML;
        var content = document.getElementById("benchmark").innerHTML;
        try {
            eval(preparation);
            eval(content);
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
        const testName: string = completedTarget.name;
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

    onErrorHandler(evt): void {
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