angular.module("teamkeep").controller("DutiesController", ["$scope", "Team", function ($scope, Team) {

    $scope.events = _.flatten(Team.Seasons, "Games");
    $scope.players = Team.playersWithName();
    $scope.eventTypeIcon = Team.eventTypeIcon;
    
    $scope.eventPredicate = function (event) {
        return (event.DateTime) ? new Date(event.DateTime) : new Date(0);
    };
    $scope.playerName = function(playerId) {
        var player = _.find($scope.players, function (p) { return p.Id == playerId; });
        return (player.FirstName + " " + player.LastName).substr(0, 27);
    };
    $scope.assignDuty = function(duty, player) {
        duty.PlayerId = player.Id;
    };
    $scope.addDuty = function(event) {
        Team.addEventDuty(event, $scope.players[0].Id, "");
    };
    $scope.removeDuty = function (duty) {
        Team.removeEventDuty(duty);
    };

}]);