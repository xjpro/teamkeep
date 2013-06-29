angular.module("teamkeep").controller("DutiesController", ["$scope", "Team", function ($scope, Team) {

    $scope.seasons = Team.Seasons;
    $scope.players = _.flatten(Team.PlayerGroups, 'Players');
    $scope.updating = function () { return Team.updating; };
    $scope.events = [];
    $scope.eventTypeIcon = Team.eventTypeIcon;   
    
    $scope.selectedSeason = null;
    $scope.unselectedSeasons = [];
    $scope.selectSeason = function (season) {
        $scope.selectedSeason = season;
        $scope.unselectedSeasons = _.reject($scope.seasons, function (season) { return season.Id == $scope.selectedSeason.Id; });
        $scope.events = $scope.selectedSeason.Games;
    };
    $scope.selectSeason($scope.seasons[0]); // init

    $scope.eventPredicate = function (event) {
        return (event.DateTime) ? new Date(event.DateTime) : new Date(0);
    };
    $scope.playerName = function (playerId) {
        var player = _.find($scope.players, function (p) { return p.Id == playerId; });
        if (!player) return "Unassigned";
        if (!player.FirstName && !player.LastName) return "[Unnamed player]";
        return ((player.FirstName || "") + " " + (player.LastName || "")).substr(0, 27);
    };
    $scope.assignDuty = function(duty, player) {
        duty.PlayerId = (player) ? player.Id : null;
    };
    $scope.addDuty = function(event) {
        Team.addEventDuty(event, null, "");
    };
    $scope.removeDuty = function (duty) {
        Team.removeEventDuty(duty);
    };

}]);