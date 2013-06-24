angular.module("teamkeep").controller("MessageController", ["$scope", "$routeParams", "Team", function ($scope, $routeParams, Team) {
    $scope.message = _.find(Team.Messages, function(message) { return message.Id == $routeParams.messageId; });
}]);