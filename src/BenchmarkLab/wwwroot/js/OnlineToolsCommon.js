const raw_input_element_name = "raw_input";
const formatted_output_element_name = "formatted_output";
const enlighterjs_wrapper_class = "EnlighterJSWrapper";
const formatted_output_textarea_element_name = "formatted_output_text";

function removeElementsByClass(className) {
    var elements = document.getElementsByClassName(className);
    while (elements.length > 0) {
        elements[0].parentNode.removeChild(elements[0]);
    }
}

function syntaxHighlightOutput(element_id, language) {
    // create config
    var options = {
        language: language,
        theme: 'classic',
        indent: 4,
        windowButton: true,
        rawButton: true,
        rawcodeDoubleclick: true,
    };

    // create new instance
    var enlighter = new EnlighterJS(document.getElementById(element_id), options);
    enlighter.enlight(true);
}

function doClearAll() {
    var input = document.getElementById(raw_input_element_name).value = '';
    removeElementsByClass(enlighterjs_wrapper_class);
    document.getElementById(formatted_output_textarea_element_name).value = '';
}

window.addEventListener('load', function () {
    document.getElementById("btn_format").addEventListener("click", doFormat);
    document.getElementById("btn_clear_all").addEventListener("click", doClearAll);
});


function doFormat() {
    var error_message = document.getElementById("error_message");
    error_message.style.display = 'none';
    var input = document.getElementById(raw_input_element_name).value;
    removeElementsByClass(enlighterjs_wrapper_class);
    try {
        var outputToTextArea = false;
        if (typeof shouldOutputToTextarea == 'function') {
            outputToTextArea = true;
        }
        if (outputToTextArea) {
            // TODO: syntax highlight text area
            document.getElementById(formatted_output_textarea_element_name).value = doFormatImpl();
            document.getElementById(formatted_output_textarea_element_name).style.display = "block";
        } else {
            document.getElementById(formatted_output_element_name).innerHTML = doFormatImpl();
            syntaxHighlightOutput(formatted_output_element_name, getLanguage())
        }
    } catch (e) {
        error_message.style.display = 'block';
        error_message.textContent = "Error:\n" + e;
    }
}