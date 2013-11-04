angular.module("teamkeep").controller("ScheduleEventController", ["$scope", "$routeParams", "$location", "Team", function($scope, $routeParams, $location, Team) {
    $scope.event = _.find(_.flatten(Team.Seasons, "Games"), function (event) { return event.Id == $routeParams.eventId; });
    $scope.editable = Team.Editable;
    $scope.settings = Team.Settings;
    $scope.eventTypes = [
        { name: "Game", value: 0 },
        { name: "Practice", value: 1 },
        { name: "Meeting", value: 2 },
        { name: "Party", value: 3 },
        { name: "None", value: 99 }
    ];
    $scope.removeEvent = function (event) {
        Team.Seasons.removeItem(event);
        $location.path("/schedule");
    };
    $scope.$watch("isMobile", function (isMobile) {
        if (!isMobile) {
            $location.path("/schedule");
        }
    });
}]);