angular.module("teamkeep").controller("UserSettingsController", function ($scope, $location, $http, User) {

    if (!User.Id) {
        $location.path("/schedule");
    }

    $scope.email = User.Email;
    $scope.verificationSent = false;
    $scope.verified = User.Verified;

    $scope.resendVerification = function () {
        $http.post("/users/verify/resend")
            .success(function () {
                $scope.verificationSent = true;
            });
    };
});