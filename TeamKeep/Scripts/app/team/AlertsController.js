angular.module("teamkeep").controller("AlertsController", ["$scope", "User", function ($scope, User) {

    $scope.resent = false;
    $scope.closeVerification = false;    
    $scope.verificationVisible = function () {
        return !$scope.closeVerification && !User.Verified;
    };
    $scope.resendVerification = function() {
        User.sendVerification()
            .success(function (wasSent) {
                if (!wasSent) {
                    $("#alert-modal").fadeAlert("show", "Looks like you already verified your email, thanks!", "alert-success"); // TODO badddd
                    $scope.closeVerification = true;
                } else {
                    $scope.resent = true;
                }
            });
    };
    $scope.checkVerification = function () {
        User.getVerification()
            .success(function (responseUser) {
                User.Verified = responseUser.Verified;
                if(User.Verified) {
                    $("#alert-modal").fadeAlert("show", "Thanks! Your email was verified successfully", "alert-success"); // TODO badddd
                    $scope.closeVerification = true;
                }
                else {
                    $("#alert-modal").fadeAlert("show", "Sorry, your email has not yet been verified", "alert-error"); // TODO badddd
                }
        });
    };
    $scope.$on("messageButton.click", function() {
        if (!User.Verified) {
            $scope.closeVerification = false;
        }
    });

}]);