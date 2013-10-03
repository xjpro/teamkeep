angular.module("teamkeep").factory("User", function() {
    var User = viewData.User;

    User.logout = function () {
        var now = new Date();
        now.setFullYear(now.getFullYear() - 1);
        document.cookie = "teamkeep-token=;path=/;expires=" + now.toGMTString();
        document.location = "/";
    };

    return User;
});