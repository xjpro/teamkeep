angular.module("teamkeep").controller("MessagesController", function ($scope, Team) {
    $scope.messages = Team.Messages;
    $scope.datePredicate = function (item) {
        return new Date(item.DateTime);
    };
});