angular.module("teamkeep").controller("SettingsController", ["$scope", "$http", "Team", function ($scope, $http, Team) {
    
    $scope.newName = angular.copy(Team.Name);
    $scope.privacy = Team.Privacy;
    $scope.settings = Team.Settings;

    $scope.updating = false;
    
    $scope.resultsViewOptions = [
        { value: 0, name: "Points scored" },
        { value: 1, name: "Tournament / best of" },
        /*{ Value: 2, Text: "Win / loss" },*/
        { value: 3, name: "Hide" }
    ];

    $scope.deleteTeam = function () {
        $scope.updating = true;
        $http.delete(Team.uri)
            .success(function () {
                window.location = "/home";
            });
    };

    $scope.$watch("newName", function() {
        if (!$scope.newName || $scope.newName.trim().length == 0) {
            $scope.newName = Team.Name;
        } else {
            Team.Name = $scope.newName;
        }
    });

}]);