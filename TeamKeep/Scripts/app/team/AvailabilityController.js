angular.module("teamkeep").controller("AvailabilityController", ["$scope", "$filter", "Team", function ($scope, $filter, Team) {

    $scope.groups = _.select(Team.PlayerGroups, function (group) { return group.Players.length > 0; });
    $scope.showPosition = Team.Settings.PositionColumn;

    $scope.allEvents = _(Team.Seasons).flatten("Games").filter(function (event) { return event.DateTime; }).sortBy(function (event) { return new Date(event.DateTime); }).value();
    $scope.eventsPage = [];

    $scope.eventsShown = 10;
    $scope.eventsIndex = _.findIndex($scope.allEvents, function (event) { return !$filter("isPast")(event); });

    $scope.showNext = function () {
        return $scope.allEvents.length > $scope.eventsShown;
    };
    $scope.showPrevious = function () {
        return $scope.allEvents.length > $scope.eventsShown;
    };
    $scope.eventPredicate = function (event) {
        return (event.DateTime) ? new Date(event.DateTime) : new Date(0);
    };

    var availabilityForPlayer = function (player, event) {
        if (!event) return null;
        return _.find(player.Availability, function (ab) {
            return ab.EventId == event.Id;
        });
    };

    $scope.availabilityCss = function (player, event) {

        var availability = availabilityForPlayer(player, event);
        if (availability) {
            switch (availability.RepliedStatus) {
                case 1: return "going";
                case 2: return "notgoing";
                case 3: return "maybe";
                default: return "";
            }
        }
    };
    $scope.availabilityIcon = function (player, event) {

        var availability = availabilityForPlayer(player, event);
        if (availability) {
            switch (availability.AdminStatus) {
                case 1: return "fa fa-thumbs-o-up";
                case 2: return "fa fa-ban";
                case 3: return "fa fa-question-circle";
                default: return "";
            }
        }
    };
    $scope.rotateAvailability = function (player, event) {
        
        var availability = availabilityForPlayer(player, event);
        if (availability) {
            if (availability.AdminStatus + 1 > 3) {
                availability.AdminStatus = 0;
            } else {
                availability.AdminStatus++;
            }
        } else {
            player.Availability.push({
                PlayerId: player.Id,
                EventId: event.Id,
                AdminStatus: 1
            });
        }
    };

    $scope.$watch("eventsIndex + eventsShown", function() {
        // create a current page (10 events)
        var begin = Math.max(0, $scope.eventsIndex);
        if (begin > $scope.allEvents.length - $scope.eventsShown) begin = Math.max(0, $scope.allEvents.length - $scope.eventsShown);
        var end = Math.min($scope.allEvents.length, begin + $scope.eventsShown);

        var pageOfEvents = [];
        for (var i = begin; i < end; i++) {
            pageOfEvents.push($scope.allEvents[i]);
        }
        $scope.eventsPage = pageOfEvents;
    });
}])
.filter("isPast", function() {
    return function(event) {
        return moment(event.DateTime).isBefore(moment());
    };
});