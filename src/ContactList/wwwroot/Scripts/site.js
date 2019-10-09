function DisableAjaxCaching() {
    $.ajaxSetup({ cache: false });
}

function IncludeAntiForgeryTokenInAjaxPosts() {
    $(document).ajaxSend(function(event, xhr, options) {
        if (options.type.toUpperCase() === "POST") {
            var token = $("#AntiForgeryToken input[name=__RequestVerificationToken]").val();
            xhr.setRequestHeader("RequestVerificationToken", token);
        }
    });
};

function RenderMvcValidationClassesWithBootstrapEquivalent() {
    // ASP.NET includes CSS classes on invalid form inputs and validation
    // messages, but those CSS classes are unstyled and merely act as
    // metadata. When those CSS classes are present, add the corresponding
    // Bootstrap CSS classes to actually style the elements and bring
    // the validation errors to the user's attention.

    $(".input-validation-error").addClass("is-invalid");
    $(".field-validation-error").addClass("invalid-feedback");
}

function InitializeConfirmationModalBehaviors() {
    $("#confirmationModal").on("show.bs.modal", function(event) {
        var $launchModalButton = $(event.relatedTarget);

        $("#confirmationTitle").text($launchModalButton.data("confirmation-title"));
        $("#confirmationPrompt").text($launchModalButton.data("confirmation-prompt"));
        $("#confirmationButton").text($launchModalButton.data("confirmation-text")).data("url", $launchModalButton.data("url"));
    });

    $('#confirmationModal').on('hide.bs.modal', function(event) {
        $("#confirmationTitle").empty();
        $("#confirmationPrompt").empty();
        $("#confirmationButton").empty().data("url", null);
    });

    $("#confirmationButton").on("click", function() {
        var $confirmationButton = $(this);

        $.ajax({ method: "POST", url: $confirmationButton.data("url") })
            .then(function(data) {
                window.location.replace(data.redirectUrl);
            })
            .catch(function(error) {
                toastr.error("An unexpected error occurred. Please refresh and try again.");
            });
    });
}

function DisplayRegisteredToastMessage() {
    $("#toast").each(function () {
        var $toast = $(this);
        var type = $toast.data("type");
        var message = $toast.val();
        toastr[type](message);
    });
}

function IdentifyRequiredFields() {
    var $form = $("form");
    $form.find("[data-val-required]").each(function () {
        var $input = $(this);
        var $label = $form.find("label[for='" + $input.attr("id") + "']");
        $label.addClass("input-required");
    });
}

function EnforceFieldMaxLengths() {
    var $form = $("form");
    $form.find("[data-val-length-max]").each(function () {
        var $input = $(this);
        $input.attr("maxlength", $input.data("val-length-max"));
    });
}

$(function() {
    DisableAjaxCaching();
    IncludeAntiForgeryTokenInAjaxPosts();
    RenderMvcValidationClassesWithBootstrapEquivalent();
    InitializeConfirmationModalBehaviors();
    DisplayRegisteredToastMessage();
    IdentifyRequiredFields();
    EnforceFieldMaxLengths();
});