/// <reference path="../typings/globals/jquery/index.d.ts" />
/// <reference path="../typings/globals/codemirror/index.d.ts" />
/// <reference path="../typings/globals/mustache/index.d.ts" />

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