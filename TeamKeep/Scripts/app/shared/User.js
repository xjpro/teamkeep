angular.module("teamkeep").factory("User", function ($http) {

    if (viewData.User == null) {
        return null;
    }

    var User = viewData.User;

    User.uri = "/users/" + User.Id;

    User.saveSettings = function () {
        $http.put(User.uri + "/settings", {
            id: User.Id,
            settings: User.Settings
        }).success(function (settings) {
            User.Settings = settings;
        });
    };

    return User;
});