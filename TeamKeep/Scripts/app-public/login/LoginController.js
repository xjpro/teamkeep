angular.module("teamkeep-public").controller("LoginController", ["$scope", "User", function ($scope, User) {

    $scope.loginEvaluating = false;
    $scope.loginError = "";
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
                TeamKeep.signIn(login.AuthToken, login.Redirect);
            }).error(function (error) {
                $scope.loginError = JSON.parse(error);
                $scope.loginEvaluating = false;
            });
    };

    $scope.resetEvaluating = false;
    $scope.resetError = "";
    $scope.resetPassword = function() {

        $scope.resetError = "";
        
        if (!$scope.username || $scope.username.trim().length < 3) {
            $scope.resetError = "Please enter your username";
            return;
        }

        $scope.resetEvaluating = true;

        User.resetPassword($scope.username)
            .success(function () {
                document.location = "/resetsent";
            }).error(function (error) {
                $scope.resetError = JSON.parse(error);
                $scope.resetEvaluating = false;
            });
    };
}]);