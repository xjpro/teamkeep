angular.module("teamkeep").factory("User", function () {

    var User = (window.viewData && window.viewData.User) ? window.viewData.User : {
        Id: 0,
        Settings: {},
        Verified: true,
        Teams: []
    };

    User.logout = function () {
        var now = new Date();
        now.setFullYear(now.getFullYear() - 1);
        document.cookie = "teamkeep-token=;path=/;expires=" + now.toGMTString();
        document.location = "/";
    };

    return User;
});