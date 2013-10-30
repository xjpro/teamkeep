angular.module("teamkeep-public").factory("User", ["$http", function($http) {

    var User = (window.viewData && window.viewData.User) ? window.viewData.User : {
        Id: 0,
        Settings: {},
        Verified: true
    };

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

    User.resetPassword = function(resetToken, username, password) {
        return $http.put("/users/password", {
            resetToken: resetToken,
            username: username.trim(),
            password: password
        });
    };

    User.requestResetPassword = function (username) {
        return $http.post("/users/password", {
            username: username.trim(),
        });
    };

    return User;
}]);