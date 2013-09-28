angular.module("teamkeep").controller("ScheduleEventController", function($scope, $routeParams, $location, Team) {
    $scope.event = _.find(_.flatten(Team.Seasons, "Games"), function (event) { return event.Id == $routeParams.eventId; });
    $scope.removeEvent = function (event) {
        Team.Seasons.removeItem(event);
        $location.path("/schedule");
    };
});