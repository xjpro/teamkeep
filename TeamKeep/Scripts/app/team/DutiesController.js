angular.module("teamkeep").controller("DutiesController", function ($scope, Team) {

    $scope.seasons = Team.Seasons;
    $scope.selectedSeason = Team.Seasons[0];
    $scope.players = _.flatten(Team.PlayerGroups, 'Players');

});