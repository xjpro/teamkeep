angular.module("teamkeep").controller("MessagesController", function($scope, $location, $routeParams, Team, User) {

    if (!User.Verified) {
        $location.path("/user");
    }

    if ($scope.$eval($routeParams.sent)) {
        $scope.sentMessage = "Your message was sent successfully";
    }

    $scope.messages = Team.Messages;
    $scope.datePredicate = function (item) {
        return new Date(item.DateTime);
    };
});