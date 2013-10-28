angular.module("teamkeep").controller("UserSettingsController", function ($scope, $http, User) {
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