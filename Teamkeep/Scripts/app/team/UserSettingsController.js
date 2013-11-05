angular.module("teamkeep").controller("UserSettingsController", ["$scope", "$location", "$http", "User", function ($scope, $location, $http, User) {

    if (!User.Id) {
        $location.path("/schedule");
    }

    $scope.email = User.Email;
    $scope.newEmail = $scope.email;
    $scope.verificationSent = false;
    $scope.verified = User.Verified;

    $scope.resendVerification = function () {
        $http.post("/users/verify/resend")
            .success(function () {
                $scope.verificationSent = true;
            });
    };

    $scope.saveChanges = function () {

        $scope.successMessage = "";
        $scope.errorMessage = "";

        $http.put("/users/" + User.Id + "/email", {
            email: $scope.newEmail
        })
        .success(function (response) {
            User.Email = response.Email;
            User.Verified = false;
            $scope.email = User.Email;
            $scope.newEmail = User.Email;
            $scope.successMessage = "Your email was updated successfully, check your inbox for verification instructions";
        })
        .error(function (response) {
            $scope.errorMessage = $scope.$eval(response);
        });
    };
}]);