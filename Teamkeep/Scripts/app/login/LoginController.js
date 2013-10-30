angular.module("teamkeep-public").controller("LoginController", ["$scope", "User", function ($scope, User) {

    var userMatches = /username=([^&]+)&?/.exec(location.search);
    if (userMatches) {
        $scope.username = userMatches[1];
    }

    $scope.loginEvaluating = false;
    $scope.loginUser = function () {

        $scope.loginError = "";
        
        if (!$scope.username || $scope.username.trim().length < 3) {
            $scope.loginError = "Please enter your username";
            return;
        }
        if (!$scope.password || $scope.password.trim().length < 3) {
            $scope.loginError = "Please enter your password";
            return;
        }
        
        $scope.loginEvaluating = true;

        User.login($scope.username, $scope.password)
            .success(function (login) {
                Teamkeep.signIn(login.AuthToken, login.Redirect);
            }).error(function (error) {
                $scope.loginError = JSON.parse(error);
                $scope.loginEvaluating = false;
            });
    };

    $scope.sendResetEvaluating = false;
    $scope.sendResetPassword = function() {

        $scope.sendResetError = "";
        
        if (!$scope.username || $scope.username.trim().length < 3) {
            $scope.sendResetError = "Please enter your username";
            return;
        }

        $scope.sendResetEvaluating = true;

        User.requestResetPassword($scope.username)
            .success(function () {
                document.location = "/resetsent";
            }).error(function (error) {
                $scope.sendResetError = $scope.$eval(error);
                $scope.sendResetEvaluating = false;
            });
    };

    $scope.resetPassword = function () {

        var tokenMatches = /token=([^&]+)&?/.exec(location.search);
        if (tokenMatches) {
            var token = tokenMatches[1];
        }
        else {
            return;
        }

        $scope.resetEvaluating = true;

        User.resetPassword(token, $scope.username, $scope.newPassword)
            .success(function (response) {
                location = "/";
            })
            .error(function (response) {
                $scope.resetEvaluating = false;
                $scope.resetError = $scope.$eval(response);
            });
    };
}]);