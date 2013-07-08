angular.module("teamkeep").factory("User", ["$http", function($http) {

    var User = (window.viewData && window.viewData.User) ? window.viewData.User : {
        Id: 0,
        Settings: {},
        Verified: true
    };

    User.uri = "/users/" + User.Id;

    User.saveEmail = function () {
        return $http.put(User.uri + "/email", {
            id: User.Id,
            email: User.Email
        }).success(function (responseUser) {
            User.Email = responseUser.Email;
            User.Verified = responseUser.Verified;
        });
    };

    User.sendVerification = function() {
        return $http.post("/users/verify/resend");
    };
    
    User.getVerification = function () {
        return $http.get("/users/active")
            .success(function(isVerified) {
                User.Verified = isVerified;
            });
    };

    User.saveSettings = function () {
        return $http.put(User.uri + "/settings", {
            id: User.Id,
            settings: User.Settings
        }).success(function (settings) {
            User.Settings = settings;
        });
    };

    return User;
}]);