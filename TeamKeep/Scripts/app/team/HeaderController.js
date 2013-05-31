angular.module("teamkeep").controller("HeaderController", ["$scope", "$rootScope", "User", function ($scope, $rootScope, User) {

    $scope.error = "";
    $scope.saveEmail = function () {

        $scope.error = "";

        if (!$scope.email || $scope.email.trim().length < 6) {
            $scope.error = "Please enter a valid email address";
            return;
        }
        $scope.email = $scope.email.trim();

        $scope.evaluating = true;

        User.Email = $scope.email;
        User.saveEmail()
            .success(function (user) {
                
                $scope.evaluating = false;
                $("#user-email-modal").modal("hide"); // TODO this is badddd

                user.Email = user.Email;
                user.Verified = user.Verified;
            })
            .error(function (error) {
                $scope.error = JSON.parse(error);
                $scope.evaluating = false;
            });
    };

    $scope.messageButton = function () {

        if (!User.Email) {
            $("#user-email-modal").modal("show"); // TODO agian, bad
            return false;
        }

        $rootScope.$broadcast("messageButton.click");
        return User.Verified;
    };

}]);