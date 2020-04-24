raw_input_element_name = "raw_input";
formatted_output_element_name = "formatted_output";
formatted_output_textarea_element_name = "formatted_output_text";

function removeElementsByClass(className) {
    var elements = document.getElementsByClassName(className);
    while (elements.length > 0) {
        elements[0].parentNode.removeChild(elements[0]);
    }
}

function doClearAll() {
    document.getElementById(raw_input_element_name).value = '';
    document.getElementById(formatted_output_textarea_element_name).value = '';
    removeElementsByClass("CodeMirror");
}

window.addEventListener('load', function () {
    document.getElementById("btn_format").addEventListener("click", doFormat);
    document.getElementById("btn_clear_all").addEventListener("click", doClearAll);
});


function doFormat() {
    var error_message = document.getElementById("error_message");
    error_message.style.display = 'none';
    removeElementsByClass("CodeMirror");
    try {
        document.getElementById(formatted_output_textarea_element_name).value = doFormatImpl();
        CodeMirror.fromTextArea(document.getElementById(formatted_output_textarea_element_name), {
            lineNumbers: true,
            mode: getLanguage(),
        });
    } catch (e) {
        error_message.style.display = 'block';
        error_message.textContent = "Error:\n" + e;
    }
}