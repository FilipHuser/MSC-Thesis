// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var jq = jQuery.noConflict();

function CallMethod(methodName, controller, onSuccess, data = {}) {
    var url = '/' + controller + '/' + methodName;

    return jq.ajax({
        url: url,
        type: 'POST',
        contentType: "application/json",
        data: JSON.stringify(data)
    }).then(
        function () { return true; },
        function () { return false; }
    );
}

function loadPartialView(partialViewName, controller, elementID, optionalParams = {}) {
    // Construct the URL to fetch the partial view
    var url = '/' + controller + '/' + partialViewName;

    // Make the AJAX request
    jq.ajax({
        url: url,
        type: 'GET',
        data: optionalParams, // Optional parameters to send in the request
        success: function (response) {
            // On success, inject the response (partial view) into the element with the given ID
            jq('#' + elementID).html(response);
        },
        error: function (xhr, status, error) {
            // Handle any errors that occur during the request
            console.error('Error loading partial view: ', status, error);
            jq('#' + elementID).html('<p>Error loading content. Please try again later.</p>');
        }
    });
}