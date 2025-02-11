// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var jq = jQuery.noConflict();

function callMethod(method, controller, data = {}) {
    var url = '/' + controller + '/' + method;

    // Get the anti-forgery token value
    var token = $('input[name="__RequestVerificationToken"]').val();

    // Send the AJAX request
    return jq.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify(data),  // Serialize data as JSON
        contentType: 'application/json',  // Ensure content type is JSON
        headers: {
            'RequestVerificationToken': token  // Add token to headers
        },
    }).then(
        function () { return true; },
        function () { return false; }
    );
}

function loadPartialView(partialViewName, controller, elementID, data = {}) {
    var url = '/' + controller + '/' + partialViewName;

    // Make the AJAX request
    jq.ajax({
        url: url,
        type: 'GET',
        data: data,
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