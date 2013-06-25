angular.module("teamkeep").controller("DutiesController", ["$scope", "Team", function ($scope, Team) {

    $scope.events = _.flatten(Team.Seasons, "Games");
    $scope.players = Team.playersWithName();
    $scope.eventTypeIcon = Team.eventTypeIcon;
    
    $scope.playerName = function(playerId) {
        var player = _.find($scope.players, function (p) { return p.Id == playerId; });
        return player.FirstName + " " + player.LastName;
    };

}]);