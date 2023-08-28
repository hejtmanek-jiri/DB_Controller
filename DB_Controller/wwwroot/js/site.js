// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showPopUpMessage(message, messageType) {
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-top-center",
        "progressBar": true,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
    };

    toastr[messageType](message);
}