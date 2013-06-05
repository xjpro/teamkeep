angular.module("teamkeep").controller("SettingsController", ["$scope", "Team", function ($scope, Team) {

    $scope.privacy = Team.Privacy;
    $scope.settings = Team.Settings;

    $scope.$watch("[privacy, settings]", function (value, oldValue) {
        if (angular.equals(value, oldValue)) return;
        Team.saveSettings();
    }, true);

}]);