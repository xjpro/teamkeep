angular.module("teamkeep").controller("SettingsController", function ($scope, Team) {
    $scope.newName = Team.Name;
    $scope.settings = Team.Settings;
    $scope.privacy = Team.Privacy;

    $scope.$watch("newName", function(value) {
        if (!value) {
            $scope.newName = Team.Name;
        }
    });
});