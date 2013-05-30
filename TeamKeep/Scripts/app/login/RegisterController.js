angular.module("teamkeep").controller("RegisterController", function ($scope, User) {

    $scope.evaluating = false;
    $scope.error = "";
    $scope.registerUser = function () {

        $scope.error = "";
        
        if (!$scope.username || $scope.username.trim().length < 3) {
            $scope.error = "Username must be at least three characters in length";
            return;
        }
        $scope.username = $scope.username.trim();

        if (!$scope.email || $scope.email.trim().length < 6) {
            $scope.error = "Please enter a valid email address";
            return;
        }
        $scope.email = $scope.email.trim();

        if (!$scope.password || $scope.password.length < 2) {
            $scope.error = "Password must be at least two characters in length";
            return;
        }

        $scope.evaluating = true;

        User.register($scope.username, $scope.email, $scope.password)
            .success(function(login) {
                TeamKeep.signIn(login.AuthToken, login.Redirect);
            })
            .error(function (error) {
                $scope.error = JSON.parse(error);
                $scope.evaluating = false;
            });
    };
});