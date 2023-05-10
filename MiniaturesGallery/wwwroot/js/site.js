// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function redirectToPage() {
    var number = prompt("Enter the target page number:", "1");
    var link = document.getElementById("hiddenLink").href;
    if (number != null && number != "" && link != null) {
        location.href = (link + "&pageNumber=" + number);
    }
}