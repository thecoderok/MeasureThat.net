/// <reference path="../typings/globals/jquery/index.d.ts" />
/// <reference path="../typings/globals/codemirror/index.d.ts" />
/// <reference path="../typings/globals/mustache/index.d.ts" />
/// <reference path="../typings/globals/google.visualization/index.d.ts" />
/// <reference path="../typings/globals/benchmark/index.d.ts" />
/// <reference path="../typings/globals/bootstrap/index.d.ts" />

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
        (window as any)._benchmark_listener = this;

        document.getElementById('runTest').removeAttribute('disabled');
        document.getElementById('runTest').addEventListener('click', this.startRun);
        (window as any)._benchmark_listener = this;
    }

    private initialize(): void {
        $("#fork-btn")
            .click(() => {
                $("#fork-form").submit();
            });
        this.createEditors();
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

    startRun(): void {
        var iframe = document.getElementById('test-runner-iframe') as HTMLIFrameElement;
        iframe.contentWindow.postMessage("start_test", "*");
    }

    handleRunCompleted(suites: Event): void {
        // typings for benchmark.js are bad  and not complete
        var benchmark = suites.currentTarget as any;

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
                $("#results-placeholder").show();
            }
        });

        google.charts.load("current", { packages: ["corechart", "bar"] });
        google.charts.setOnLoadCallback(() => ShowPageController.drawChart(chartData));
        
        $("#runTest").removeAttr("disabled");
        document.getElementById('spinner').style.display = 'none';
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

    initialize() {
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

class ForkBenchmarkHandler {
    constructor() {
        $("[data-role='fork-benchmark']").on("click", this.handleForkButtonClick);
        $("#perform-fork").on("click", this.performFork);
    }

    private handleForkButtonClick(eventObject: JQueryEventObject): void {
        var id: string = eventObject.target.getAttribute("data-entity-id");
        if (id === '') {
            throw new Error("Can't get id of the benchmark");
        }
        $("#fork-form [name='id']").val(id);
        $("#fork-confirm").modal();
    }

    private performFork(): void {
        $("#fork-form form").submit();
    }
}

class AppendSnippetHandler {
    constructor() {
        $(document).ready(() => this.initialize());
    }

    private initialize(): void {
        $("[data-role='insert-snippet']").on("click", this.handleClick);
    }

    private handleClick(eventObject: JQueryEventObject): void {
        var target: string = eventObject.target.getAttribute("data-target");
        if (target === '') {
            throw new Error("Can't get target");
        }

        var snippet: string = eventObject.target.getAttribute("data-text");
        if (snippet === '') {
            throw new Error("Can't get snippet");
        }

        var targetEl = document.getElementById(target) as HTMLTextAreaElement;
        var editor = (targetEl.nextSibling as any).CodeMirror as CodeMirror.EditorFromTextArea;
        editor.setValue(editor.getValue() + "\n" + snippet);
        editor.save();
    }
}

class ClientValidationHandler {

    constructor() {
        $('#BenchmarkName').change(this.validate);
        $('#dup-title').hide();
        this.validate();
        $('[data-role="test-benchmark"]').on('click', () => this.handleValidateButton());
        (window as any)._validation_handler = this;
    }

    validationFailed(errorMessage: string): void {
        alert('Benchmark failed during validation.\nError: ' + errorMessage);
        this.setValidateBtnState(true);
    }

    validationSuccess(): void {
        this.setValidateBtnState(true);
        $('#benchmark_submit').show();
    }

    private setValidateBtnState(enabled: boolean): void {
        if (enabled) {
            $('[data-role="test-benchmark"]').removeAttr('disabled');
            $('#validate-spinner').hide();
        } else {
            $('[data-role="test-benchmark"]').attr('disabled', 'true');
            $('#validate-spinner').show();
        }
    }
    private handleValidateButton(): void {
        this.setValidateBtnState(false);
        $('#benchmark_submit').hide();

        const form = $("#new-benchmark-form");
        const url: string = "/Benchmarks/ValidateBenchmark";
        const _thisMy = this;
        $.ajax({
            type: "POST",
            url: url,
            cache: false,
            data: form.serialize(), // serializes the form's elements.
            success(data) {
                if (!data) {
                    alert('Invalid validation response!');
                    _thisMy.setValidateBtnState(true);
                } else if (data.error) {
                    alert('Error occurred during validation: ' + data.error);
                    _thisMy.setValidateBtnState(true);
                } else if (data.valid === true) {
                    _thisMy.continueBenchmarkValidation();
                } else if (data.valid === false) {
                    alert('Benchmark is not valid.\nErrors: ' + JSON.stringify(data.errors));
                    _thisMy.setValidateBtnState(true);
                } else {
                    alert('Invalid validation response!');
                    _thisMy.setValidateBtnState(true);
                }
            },
            error(e) {
                alert('Error occurred during validation: ' + JSON.stringify(e));
                _thisMy.setValidateBtnState(true);
            }
        });
    }

    private continueBenchmarkValidation(): void {
        // Cleanup previous validation frames
        $('#validation-iframe').remove();

        // add new one
        const html = '<iframe id="validation-iframe" src="/Benchmarks/TestFrameForValidation?autostart=1" style="border:none; max-height: 2px; overflow:none"></iframe>';
        $('#validation-frame-holder').html(html);
    }

    allowSave(): void {
        this.setValidateBtnState(true);
        $('#benchmark_submit').show();
    }

    private validate(): void {
        var value: string = $('#BenchmarkName').val();
        if (value === '') {
            $('#dup-title').hide();
            return;
        }

        var uri = '/api/CheckBenchmarkTitle?title=';
        $.getJSON(uri + encodeURIComponent(value))
            .done(function (data) {
                if (data === true) {
                    $('#dup-title').show();
                } else {
                    $('#dup-title').hide();
                }
            });
    }
}