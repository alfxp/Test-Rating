$(document).ready(function () {

    getRating();

    function getRating() {

        $.ajax({
            url: "http://localhost:53158/api/rating/getRating",
            type: 'POST',
            data: window.formData,

            processData: false,
            contentType: false,

            success: function (data) {
                console.log(data);
            },
            error: function () {
                console.log("not so boa!");
            }
        });

    }

    function setRating() {

        $.ajax({
            url: "http://localhost:51629/Rating/set",
            type: 'POST',
            data: window.formData,

            processData: false,
            contentType: false,

            success: function (data) {
                console.log(data);
            },
            error: function () {
                console.log("not so boa!");
            }
        });

    }
});

