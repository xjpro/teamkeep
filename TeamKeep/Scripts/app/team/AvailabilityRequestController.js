﻿angular.module("teamkeep").controller("AvailabilityRequestController", ["$scope", "$http", "Team", function ($scope, $http, Team) {

    $scope.groups = Team.PlayerGroups;
    $scope.event = Team.selectedEvent;
    $scope.requesting = false;

    $scope.eventHeading = function () {
        if (!Team.selectedEvent) return "";
        return Team.Name + ' vs. ' + (Team.selectedEvent.OpponentName || '[To Be Determined]');
    };
    $scope.eventWhen = function () {
        if (!Team.selectedEvent) return "";
        if (!Team.selectedEvent.DateTime) return '[To Be Determined]';
        return moment(Team.selectedEvent.DateTime).format("dddd MMMM Do, YYYY @ h:mma");
    };
    $scope.eventWhere = function () {
        if (!Team.selectedEvent) return "";
        if (!Team.selectedEvent.Location) return '[To Be Determined]';

        var where = [];
        if (Team.selectedEvent.Location.Description) where.push(Team.selectedEvent.Location.Description + "<br/>");
        if (Team.selectedEvent.Location.Street) where.push(Team.selectedEvent.Location.Street + "<br/>");
        if (Team.selectedEvent.Location.City) where.push(Team.selectedEvent.Location.City + "<br/>");
        if (Team.selectedEvent.Location.Postal) where.push(Team.selectedEvent.Location.Postal + "<br/>");
        return where.join('');
    };

    $scope.selectedMembers = function() {
        var members = _.flatten(Team.PlayerGroups, "Players");
        return _.filter(members, function(member) { return member.selected; });
    };

    $scope.toggleGroupSelected = function(group) {
        if (!group.selected) {
            group.selected = true;
            _.each(group.Players, function(member) { member.selected = true; });
        } else {
            group.selected = false;
            _.each(group.Players, function (member) { member.selected = false; });
        }
    };
    $scope.toggleSelected = function(member) {
        member.selected = true;
    };

    $scope.availabilityEmailSent = function (member) {
        if (!Team.selectedEvent) return false;
        var ab = _.find(member.Availability, function (otherAb) { return otherAb.EventId == Team.selectedEvent.Id; });
        return (ab != null && ab.EmailSent != null);
    };

    $scope.sendRequests = function() {

        $scope.requesting = true;

        $http.post("/games/" + Team.selectedEvent.Id + "/confirmations", {
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
        })
        .error(function(errorMessage) {
            $("#alert-modal").fadeAlert("show", JSON.parse(errorMessage), "alert-error"); // TODO should go in directive or view?
            $scope.requesting = false;
        });
    };

}]);