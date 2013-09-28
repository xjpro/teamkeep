﻿angular.module("teamkeep").controller("AvailabilityRequestController", ["$scope", "$routeParams", "$http", "$location", "Team", function ($scope, $routeParams, $http, $location, Team) {

    $scope.groups = Team.PlayerGroups;
    $scope.event = _(Team.Seasons).flatten("Games").find(function(event) { return event.Id == $routeParams.eventId; });
    $scope.requesting = false;

    $scope.eventHeading = (function () {
        switch ($scope.event.Type) {
            case 0: return Team.Name + ' vs. ' + ($scope.event.OpponentName || '[To Be Determined]');
            case 1:
            case 2:
            case 3:
                return Team.eventTypeTitle($scope.event.Type) + ($scope.event.OpponentName ? " — " + $scope.event.OpponentName : "");
            default: return Team.Name + " — " + $scope.event.OpponentName;
        }
    })();
    $scope.eventWhen = (!$scope.event.DateTime) ? '[To Be Determined]' : moment($scope.event.DateTime).format("dddd MMMM Do, YYYY @ h:mma");
    $scope.eventWhere = (function () {
        if (!$scope.event.Location) return '[To Be Determined]';

        var where = [];
        if ($scope.event.Location.Description) where.push($scope.event.Location.Description + "<br/>");
        if ($scope.event.Location.Street) where.push($scope.event.Location.Street + "<br/>");
        if ($scope.event.Location.City) where.push($scope.event.Location.City + "<br/>");
        if ($scope.event.Location.Postal) where.push($scope.event.Location.Postal + "<br/>");
        return where.join('');
    })();

    $scope.selectedMembers = function() {
        var members = _.flatten(Team.PlayerGroups, "Players");
        return _.filter(members, function(member) { return member.selected; });
    };

    angular.forEach($scope.groups, function(group) {
        $scope.$watch(function () { return group.selected; }, function(value) {
            if (value) {
                _.each(group.Players, function(member) { member.selected = true; });
            } else {
                _.each(group.Players, function (member) { member.selected = false; });
            }
        });
    });

    $scope.availabilityEmailSent = function (member) {
        var ab = _.find(member.Availability, function (otherAb) { return otherAb.EventId == $scope.event.Id; });
        return (ab != null && ab.EmailSent != null);
    };

    $scope.sendRequests = function() {

        $scope.requesting = true;

        $http.post("/games/" + $scope.event.Id + "/confirmations", {
            playerIds: _.pluck($scope.selectedMembers(), 'Id')
        })
        .success(function(response) {

            var members = _.flatten(Team.PlayerGroups, "Players");

            _.each(response.UpdatedAvailabilities, function (ab) {
                var member = _.find(members, function(other) { return other.Id == ab.PlayerId; });
                if (member != null) {
                    member.Availability.splice(_.findIndex(member.Availability, function(otherAb) { return otherAb.EventId == ab.EventId; }), 1);
                    member.Availability.push(ab);
                }
            });

            _.each(members, function (member) { member.selected = false; }); // Remove selections
            
            $("#alert-modal").fadeAlert("show", "Availability requests sent successfully", "alert-success"); // TODO should go in directive or view?
            $scope.requesting = false;

            $location.path("/availability");
        })
        .error(function(errorMessage) {
            $("#alert-modal").fadeAlert("show", JSON.parse(errorMessage), "alert-error"); // TODO should go in directive or view?
            $scope.requesting = false;
        });
    };

}]);