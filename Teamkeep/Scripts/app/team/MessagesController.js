angular.module("teamkeep").controller("MessagesController", function ($scope, $location, Team, User) {
    
    if (!User.Verified) {
        $location.path("/user");
    }

    $scope.messages = Team.Messages;
    $scope.datePredicate = function (item) {
        return new Date(item.DateTime);
    };
});