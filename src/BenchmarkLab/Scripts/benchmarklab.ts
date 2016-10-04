/// <reference path="../typings/globals/jquery/index.d.ts" />
/// <reference path="../typings/globals/codemirror/index.d.ts" />
/// <reference path="../typings/globals/mustache/index.d.ts" />
/// <reference path="../typings/globals/google.visualization/index.d.ts" />
/// <reference path="../typings/globals/benchmark/index.d.ts" />
/// <reference path="../typings/globals/bootstrap/index.d.ts" />

//import { Event, Suite } from "benchmark";

// TODO: this logic to migrate to Angular

class AddNewTestPageController {
    testCaseCounter: number = 0;

    constructor() {
        $(document).ready(() => this.initialize());
    }

    // Deletes test case input panel from the page
    private deleteTest(testCase: Element): boolean {
        $(testCase.parentNode.parentNode).remove();
        return false;
    }

    // Creates new test case input panel
    private makeNewTestCase(strTestCaseContent: string, strTestCaseName: string): void {
        if (!strTestCaseContent) {
            strTestCaseContent = "";
        }

        if (!strTestCaseName) {
            strTestCaseName = "";
        }

        var template = $('#testCase').html();
        Mustache.parse(template);   // optional, speeds up future uses
        var rendered = Mustache.render(template,
        {
            textCaseContent: strTestCaseContent,
            testCaseName: strTestCaseName,
            testCaseId: this.testCaseCounter++
        });

        var newTestCase = $(rendered);
        $("#test-case-list").append(newTestCase);
        var editor = newTestCase.find("textarea");
        var ed = CodeMirror.fromTextArea(editor[0] as HTMLTextAreaElement, {
            lineNumbers: true,
            mode: "javascript",
            value: "\n\n\n"
        });
        ed.on("blur", instance => {
            (instance as CodeMirror.EditorFromTextArea).save();
        });

        newTestCase.find("[data-action='delete-test']")
            .click((eventObject: JQueryEventObject) => this.deleteTest(eventObject.target));
    }

    // Initialize controller: create code mirror editors and attach event handlers for new test case buttons
    private initialize(): void {
        var editor = CodeMirror.fromTextArea(document.getElementById("HtmlPreparationCode") as HTMLTextAreaElement, {
            lineNumbers: true,
            mode: "xml",
            value: "\n\n\n"
        });
        editor.on("blur", instance => {
            (instance as CodeMirror.EditorFromTextArea).save();
        });

        editor = CodeMirror.fromTextArea(document.getElementById("ScriptPreparationCode") as HTMLTextAreaElement, {
            lineNumbers: true,
            mode: 'javascript'
        });
        editor.on("blur", instance => {
            (instance as CodeMirror.EditorFromTextArea).save();
            
        });
        //editor.setValue(' \n \n \n');

        $("[data-action='new-test']").on("click", () => this.makeNewTestCase("", ""));

        // Preserve tests previously entered by user
        $("[data-content='existing-test']").each((idx, el) => {
            this.makeNewTestCase(el.textContent, $(el).attr("data-test-name"));
        });
    }
}

class ShowPageController {
    constructor() {
        $(document).ready(() => this.initialize());
    }

    private initialize(): void {
        $(document)
            .ready(() => {
                $("#fork-btn")
                    .click(() => {
                        $("#fork-form").submit();
                    });
                $("#runTest").removeAttr("disabled");
                this.createEditors();
            });
    }

    private createEditors(): void {
        // Will enable codeMirror
        $("[data-code='html']")
            .each((index: number, elem: Element) => {
                var editor = CodeMirror.fromTextArea(elem as HTMLTextAreaElement,
                {
                    lineNumbers: true,
                    mode: 'xml',
                    readOnly: true,
                    viewportMargin: Infinity
                });
            });

        $("[data-code='javascript']")
            .each((index: number, elem: Element) => {
                var editor = CodeMirror.fromTextArea(elem as HTMLTextAreaElement,
                {
                    //lineNumbers: true,
                    mode: 'javascript',
                    readOnly: true,
                    viewportMargin: Infinity
                });
            });
    }

    onStartHandler(): void {
        const suiteStatusLabels = $("[data-role='suite-status']");
        suiteStatusLabels.text('Running');
        suiteStatusLabels.attr("class", "label label-info");
        $("#runTest").attr("disabled", "true");
    }

    onCycleHandler(targets: Event) : void {
        const completedTarget = targets.target as any;
        const testName: string = completedTarget.name;
        const row = $(`[data-row-for='${testName}']`);
        if (row.length !== 1) {
            throw "Unable to find where to report result";
        }
        row.find('[data-role="result-label"]').text(completedTarget.toString());
    }

    onAbortHandler() : void {
        const suiteStatusLabels = $("[data-role='suite-status']");
        suiteStatusLabels.text('Aborted');
        suiteStatusLabels.attr("class", "label label-warning");
        $("#runTest").removeAttr("disabled");
    }

    onErrorHandler(evt) : void {
        let message = "Some error occurred.";
        if (evt && evt.target && evt.target.error) {
            message = evt.target.error;
        }
        $("#error-message").text(message);
        $("#errorDuringExecution").modal();
        const suiteStatusLabels = $("[data-role='suite-status']");
        suiteStatusLabels.text("Error");
        suiteStatusLabels.attr("class", "label label-danger");
        $("#runTest").removeAttr("disabled");
    }

    onResetHandler() : void {
        const suiteStatusLabels = $("[data-role='suite-status']");
        suiteStatusLabels.text("Reset");
        suiteStatusLabels.attr("class", "label label-warning");
        $("#runTest").removeAttr("disabled");
    }

    onCompleteHandler(suites: Event): void {
        // typings for benchmark.js are bad  and not complete
        var benchmark = suites.currentTarget as any;
        var suiteStatusLabels = $("[data-role='suite-status']");
        if ((suites.target as any).aborted === true) {
            suiteStatusLabels.text("Aborted");
            suiteStatusLabels.attr("class", "label label-warning");
            return;
        }
        suiteStatusLabels.text('Completed');
        suiteStatusLabels.attr("class", "label label-success");
        $("[data-role='fastest-label']").text(benchmark.filter("fastest").map("name"));
        $("[data-role='slowest-label']").text(benchmark.filter("slowest").map("name"));

        const form = $("#results-form");
        var chartData: Array<Array<string>> = [];
        const header: Array<string> = ["Test case", "Executions Per Second"];
        chartData.push(header);
        for (let i = 0; i < (suites.currentTarget as any).length; i++) {
            form.find(`[name='ResultRows[${i}].TestName']`).val(suites.currentTarget[i].name);
            form.find(`[name='ResultRows[${i}].NumberOfSamples']`).val(suites.currentTarget[i].stats.sample.length);
            form.find(`[name='ResultRows[${i}].ExecutionsPerSecond']`).val(suites.currentTarget[i].hz);
            form.find(`[name='ResultRows[${i}].RelativeMarginOfError']`).val(suites.currentTarget[i].stats.rme);

            const chartItem: Array<string> = [];
            chartItem.push(suites.currentTarget[i].name);
            chartItem.push(suites.currentTarget[i].hz);
            chartData.push(chartItem);

        }

        const url: string = "/Benchmarks/PublishResults";

        $.ajax({
            type: "POST",
            url: url,
            cache: false,
            data: form.serialize(), // serializes the form's elements.
            success(data) {
                $("#results-placeholder").html(data);
            }
        });

        google.charts.load("current", { packages: ["corechart", "bar"] });
        google.charts.setOnLoadCallback(() => ShowPageController.drawChart(chartData));
        
        $("#runTest").removeAttr("disabled");
    }

    private static  drawChart(chartData: Array<Array<string>>) {
        const data = google.visualization.arrayToDataTable(chartData);
        var options = {
            title: "Benchmark results",
            //width: 800,
            //height: 400,
            bar: { groupWidth: "95%" },
            legend: { position: "none" },
            vAxis: {
                minValue: 0
            }
        };
        const chart = new google.visualization.ColumnChart(document.getElementById("chart_div"));
        chart.draw(data, options);
    }
}

class DisqusComments {
    public static setupLoad(): void {
        $(document).ready(() => {
            $('.show-comments').on('click', (eventData: JQueryEventObject) => {
                var disqus_shortname = 'https-www-measurethat-net'; // Replace this value with *your* username.

                // ajax request to load the disqus javascript
                $.ajax({
                    type: "GET",
                    url: "https://" + disqus_shortname + ".disqus.com/embed.js",
                    dataType: "script",
                    cache: true
                });
                // hide the button once comments load
                $(eventData.target).fadeOut();
            });
        });
    }
}

class ShowResultsPageController {
    chartData: Array<Array<string>> = [];

    constructor() {
        $(document).ready(() => this.initialize());
    }

    initialize() {
        // Will enable codeMirror
        $("[data-code='html']")
            .each(function (index) {
                var editor = CodeMirror.fromTextArea(this,
                    {
                        lineNumbers: true,
                        mode: 'xml',
                        readOnly: true,
                        viewportMargin: Infinity
                    });
            });

        $("[data-code='javascript']")
            .each(function (index) {
                var editor = CodeMirror.fromTextArea(this,
                    {
                        //lineNumbers: true,
                        mode: 'javascript',
                        readOnly: true,
                        viewportMargin: Infinity
                    });
            });
    }

    draw() : void {
        google.charts.load('current', { packages: ['corechart', 'bar'] });
        google.charts.setOnLoadCallback(() => ShowResultsPageController.drawChart(this.chartData));
    }

    static drawChart(dataToDraw: Array<Array<string>>) : void  {
        var data = google.visualization.arrayToDataTable(dataToDraw);
        var options = {
            title: "Benchmark results",
            bar: { groupWidth: "95%" },
            legend: { position: "none" },
            vAxis: {
                minValue: 0
            }
        };
        const chart = new google.visualization.ColumnChart(document.getElementById("chart_div"));
        chart.draw(data, options);
    }
}

class PaginationController {
    private activeClass: string = "active";

    constructor() {
        // Find our pager
        var paginationRoot: JQuery = $("[data-role='pagination-root']");
        if (paginationRoot.length === 0) {
            // No pagination control found here
            console.warn("No pagination control found on the page");
            return;
        }

        paginationRoot.each((index: number, el: Element) : void => {
            var endpoint = el.getAttribute('data-endpoint');
            if (!endpoint || endpoint.length === 0) {
                console.warn("Pagination endpoint is not specified");
                return;
            }

            var target = el.getAttribute('data-target');
            if (target.length === 0) {
                console.warn("No target id specified for pagination");
                return;
            }

            // Find button for individual pages
            var buttons: JQuery = $(el).find("[data-role='page-button']");
            buttons.on("click",
                (eventObject: JQueryEventObject, ...args: any[]) => {
                    this.handlePageButtonClick(paginationRoot,
                        eventObject.target,
                        endpoint,
                        target);
                });
        });
        
    }

    private handlePageButtonClick(paginationRoot: JQuery,
        clickedButton: Element,
        endpoint: string,
        targetId: string) {
        var paneNum: string = clickedButton.getAttribute("data-page");
        var url: string = endpoint + "?page=" + paneNum;
        $(targetId).html($("#progress-hidden").html());
        $(targetId).load(url);

        // Update button state - find active button
        var activeBtn: JQuery = paginationRoot.find("." + this.activeClass);
        activeBtn.removeClass(this.activeClass);

        clickedButton.parentElement.classList.add(this.activeClass);
    }
}

class DeleteBenchmarkHandler {
    constructor() {
        $("[data-role='delete-benchmark']").on("click", this.handleDeleteButtonClick);
        $("#perform-delete").on("click", this.performDelete);
    }

    private handleDeleteButtonClick(eventObject: JQueryEventObject): void {
        var id: string = eventObject.target.getAttribute("data-entity-id");
        if (id === ''){
            throw new Error("Can't get id of the benchmark");
        }

        $("#delete-form [name='id']").val(id);
        $("#delete-confirm").modal();
    }

    private performDelete(): void{
        $("#delete-form form").submit();
    }
}