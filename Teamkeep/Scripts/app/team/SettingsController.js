angular.module("teamkeep").controller("SettingsController", ["$scope", "$http", "Team", function ($scope, $http, Team) {
    $scope.newName = Team.Name;
    $scope.settings = Team.Settings;
    $scope.privacy = Team.Privacy;

    $scope.resultsViewOptions = [
        { value: 0, name: "Points scored" },
        { value: 1, name: "Tournament / best of" },
        /*{ Value: 2, Text: "Win / loss" },*/
        { value: 3, name: "Hide" }
    ];

    $scope.$watch("newName", function(value) {
        if (!value) {
            $scope.newName = Team.Name;
        }
        else {
            Team.Name = $scope.newName;
        }
    });

    $scope.deleteTeam = function () {
        $http({
            method: "DELETE",
            url: Team.uri
        })
        .success(function () {
            window.location = "/teams"
        });
    };
}]);