angular.module("teamkeep").factory("User", ["$http", function($http) {

    var User = (window.viewData && window.viewData.User) ? window.viewData.User : {
        Id: 0,
        Settings: {}
    };

    User.uri = "/users/" + User.Id;

    User.login = function(username, password) {
        return $http.post("/login", {
            username: username.trim(),
            password: password
        });
    };

    User.register = function (username, email, password) {
        return $http.post("/users", {
            username: username.trim(),
            email: email.trim(),
            password: password
        });
    };

    User.resetPassword = function(username) {
        return $http.post("/users/password", {
           username:  username.trim()
        });
    };

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