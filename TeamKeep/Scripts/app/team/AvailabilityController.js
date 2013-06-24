angular.module("teamkeep").controller("AvailabilityController", ["$scope", "Team", function ($scope, Team) {

    $scope.groups = Team.PlayerGroups;

    $scope.allEvents = function () {
        var events = _.flatten(Team.Seasons, "Games");
        var filtered = _.filter(events, function (event) { return event.DateTime; });
        return _.sortBy(filtered, function(event) { return new Date(event.DateTime); });
    };
    $scope.events = function () {
        var events = $scope.allEvents();

        // create a current page (10 events)
        var begin = Math.max(0, $scope.eventsIndex);
        if (begin > events.length - $scope.eventsShown) begin = Math.max(0, events.length - $scope.eventsShown);
        var end = Math.min(events.length, begin + $scope.eventsShown);

        var pageOfEvents = [];
        for (var i = begin; i < end; i++) {
            pageOfEvents.push(events[i]);
        }
        return pageOfEvents;
    };
    $scope.isPast = function (event) {
        return moment(event.DateTime).isBefore(moment());
    };

    $scope.eventsShown = 10;
    $scope.eventsIndex = _.findIndex($scope.allEvents(), function (event) { return !$scope.isPast(event); });

    $scope.showNext = function () {
        return $scope.allEvents().length > $scope.eventsShown;
    };
    $scope.showPrevious = function () {
        return $scope.allEvents().length > $scope.eventsShown;
    };
    $scope.showPosition = Team.Settings.PositionColumn;
    
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
                case 1: return "icon-thumbs-up";
                case 2: return "icon-ban-circle";
                case 3: return "icon-question-sign";
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

    $scope.selectEvent = function (event) {
        Team.selectedEvent = event;
    };

}]);