/// <reference path="../typings/globals/mustache/index.d.ts" />

class TestRunnerController {
    constructor() {
        document.addEventListener("DOMContentLoaded", () => this.initialize());
    }

    private initialize(): void {
        document.getElementById('runTest').removeAttribute('disabled');
    }

    onStartHandler(): void {
        const suiteStatusLabels = document.querySelectorAll("[data-role='suite-status']")[0];
        suiteStatusLabels.textContent = 'Running';
        suiteStatusLabels.setAttribute("class", "label label-info");
        document.getElementById('runTest').setAttribute('disabled', 'true');
    }

    onCycleHandler(targets: Event): void {
        const completedTarget = targets.target as any;
        const testName: string = completedTarget.name;
        const row = document.querySelectorAll(`[data-row-for='${testName}']`);
        if (row.length !== 1) {
            throw "Unable to find where to report result";
        }
        row[0].querySelectorAll('[data-role="result-label"]')[0].textContent = completedTarget.toString();
    }

    onAbortHandler(): void {
        const suiteStatusLabels = document.querySelectorAll("[data-role='suite-status']")[0];
        suiteStatusLabels.textContent = 'Aborted';
        suiteStatusLabels.setAttribute("class", "label label-warning");
        document.getElementById('runTest').removeAttribute('disabled');
    }

    onErrorHandler(evt): void {
        let message = "Some error occurred.";
        if (evt && evt.target && evt.target.error) {
            message = evt.target.error;
        }
        alert(message);
        const suiteStatusLabels = document.querySelectorAll("[data-role='suite-status']")[0];
        suiteStatusLabels.textContent = 'Error';
        suiteStatusLabels.setAttribute("class", "label label-danger");
        document.getElementById('runTest').removeAttribute('disabled');
    }

    onResetHandler(): void {
        const suiteStatusLabels = document.querySelectorAll("[data-role='suite-status']")[0];
        suiteStatusLabels.textContent = 'Reset';
        suiteStatusLabels.setAttribute("class", "label label-warning");
        document.getElementById('runTest').removeAttribute('disabled');
    }

    onCompleteHandler(suites: Event): void {
        // typings for benchmark.js are bad  and not complete
        var benchmark = suites.currentTarget as any;
        const suiteStatusLabels = document.querySelectorAll("[data-role='suite-status']")[0];
        if ((suites.target as any).aborted === true) {
            suiteStatusLabels.textContent = 'Aborted';
            suiteStatusLabels.setAttribute("class", "label label-warning");
            return;
        }
        suiteStatusLabels.textContent = 'Completed';
        suiteStatusLabels.setAttribute("class", "label label-success");
        document.querySelectorAll("[data-role='fastest-label']")[0].textContent = benchmark.filter("fastest").map("name");
        document.querySelectorAll("[data-role='slowest-label']")[0].textContent = benchmark.filter("slowest").map("name");

        this.postResults();

        document.getElementById('runTest').removeAttribute('disabled');
    }

    postResults(): void {

    }
}