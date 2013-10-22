angular.module("teamkeep-public").controller("LoginController", ["$scope", "User", function ($scope, User) {

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

        User.resetPassword($scope.username)
            .success(function () {
                document.location = "/resetsent";
            }).error(function (error) {
                $scope.sendResetError = JSON.parse(error);
                $scope.sendResetEvaluating = false;
            });
    };

    $scope.resetPassword = function() {
        /*var ResetViewModel = function (data) {
            var me = this;
            ko.mapping.fromJS(data, {}, me);

            this.NewPassword = ko.observable("");
            this.Reset = function () {
                $("button").prop("disabled", true);
                $("button .icon-spin").show();
                $("#reset-alert").hide();
                $.ajax({
                    type: "PUT", url: "/users/password",
                    data: JSON.stringify({
                        Username: me.Username(),
                        ResetToken: me.ResetToken(),
                        Password: me.NewPassword()
                    }),
                    contentType: "application/json",
                    success: function (response) {
                        TeamKeep.signIn(response.AuthToken, response.Redirect);
                    },
                    error: function (response) {
                        $("#reset-alert").html(JSON.parse(response.responseText)).show();
                        $("button").prop("disabled", false);
                        $("button .icon-spin").hide();
                    }
                });
            };
        };

        var username = location.search.match(/username=([^&]*)/i);
        var resetHash = location.search.match(/token=([^&]*)/i);

        var resetViewData = {
            Username: (username) ? username[1] : "",
            ResetToken: (resetHash) ? resetHash[1] : ""
        };*/
    };
}]);