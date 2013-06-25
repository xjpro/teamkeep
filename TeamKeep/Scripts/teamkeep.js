
window.TeamKeep = {
    isMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(navigator.userAgent),
    signIn: function(token, redirect) {
        var now = new Date();
        now.setFullYear(now.getFullYear() + 1);
        document.cookie = "teamkeep-token=" + token.AsString + ";path=/;expires=" + now.toGMTString();
        document.location = redirect;
    }
};

$(function () {
    if (window.viewData) {
        
        var TeamViewModel = function (data) {
            ko.mapping.fromJS(data, {}, this);
        };

        if (window.viewData.Team) {
            window.teamViewModel = ko.mapping.fromJS(window.viewData.Team, ko.mapping.toViewModel(TeamViewModel));
        }
        $("#alert-modal").fadeAlert();
    }
});