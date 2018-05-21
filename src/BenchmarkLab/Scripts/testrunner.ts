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
    constructor() {
        document.addEventListener("DOMContentLoaded", () => this.initialize());
    }

    private initialize(): void {
        window.addEventListener("message", (e) => this.handleMessage(e));
        var url = window.location.href;
        if (url.indexOf('?autostart=1') != -1) {
            this.autostartTest();
        }
    }

    private autostartTest(): void {
        var outerRunner: any = (parent.window as any)._validation_handler;
        try {
            document.getElementById('validation-html-preparation').innerHTML =
                (parent.window.document.getElementById('HtmlPreparationCode') as HTMLTextAreaElement).value;
            eval((parent.document.getElementById('ScriptPreparationCode') as HTMLTextAreaElement).value);

            var tc = window.parent.document.getElementById('test-case-list').querySelectorAll('[data-role="testCaseComponent"]');
            var suite:any = eval("new Benchmark.Suite");
            for (var i = 0; i < tc.length; i++) {
                var testName = (tc[i].querySelectorAll('[data-role="testCaseName"]')[0] as HTMLInputElement).value;
                var testBody = (tc[i].querySelectorAll('[data-role="testCaseCode"]')[0] as HTMLTextAreaElement).value;
                
                suite.add(testName, function () {
                    eval(testBody);
                });
            }
            /*
             * suite.on('start', pageController.onStartHandler);
    suite.on('cycle', pageController.onCycleHandler);
    suite.on('abort', pageController.onAbortHandler);
    suite.on('error', pageController.onErrorHandler);
    suite.on('reset', pageController.onResetHandler);
    suite.on('complete', pageController.onCompleteHandler);
             */ 
            suite.on('cycle', function (event) {
                console.log(String(event.target));
            })
            .on('complete', function () {
                console.log('Fastest is ' + this.filter('fastest').map('name'));
            })
                .on('abort', function (evt) {
                    console.log('abort: ' + JSON.stringify(evt));
            })
                .on('error', function (evt) {
                    let message = "Some error occurred.";
                    if (evt && evt.target && evt.target.error) {
                        message = evt.target.error;
                    }
                    console.log('error: ' + message);
            })
                .on('reset', function (evt) {
                    console.log('reset: ' + JSON.stringify(evt));
            })
            // run async
            .run({ 'async': true });
        } catch (e) {
            outerRunner.validationFailed(JSON.stringify(e));
        }
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
            this.runTests();
        }
    }

    runTests(): void {
        // TODO: Group these in funciton to enable-disable elements
        window.parent.document.getElementById('runTest').setAttribute('disabled', 'true');
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

        var preparation = document.getElementById("jspreparation").innerHTML;
        var content = document.getElementById("benchmark").innerHTML;
        try {
            eval(preparation);
            eval(content);
        } catch (e) {
            alert("Error:" + JSON.stringify(e));
            throw e;
        }
    }

    onStartHandler(): void {
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        suiteStatusLabels.textContent = 'Running';
        suiteStatusLabels.setAttribute("class", "label label-info");
    }

    onCycleHandler(targets: Event): void {
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
        window.parent.document.getElementById('spinner').style.display = 'none';
    }

    onResetHandler(): void {
        const suiteStatusLabels = getElementByDataAttribute("[data-role='suite-status']");
        suiteStatusLabels.textContent = 'Reset';
        suiteStatusLabels.setAttribute("class", "label label-warning");
        window.parent.document.getElementById('runTest').removeAttribute('disabled');
        window.parent.document.getElementById('spinner').style.display = 'none';
    }

    onCompleteHandler(suites: Event): void {
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
        window.parent.document.getElementById('spinner').style.display = 'none';

        (window.parent as any)._benchmark_listener.handleRunCompleted(suites);
        //this.postResults();
    }
}