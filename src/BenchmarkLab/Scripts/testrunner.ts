/// <reference path="../typings/globals/mustache/index.d.ts" />

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
    }

    handleMessage(event: any): void {
        if (event.origin !== "http://localhost:5000"
            && event.origin !== "https://measurethat.net/"
            && event.origin !== "https://measurethat.net"
            && event.origin !== "https://benchmarklab.azurewebsites.net") {
            // Where did this message came from?
            return;
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