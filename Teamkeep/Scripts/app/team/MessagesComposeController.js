angular.module("teamkeep").controller("MessagesComposeController", function ($scope, $routeParams, $http, $location, Team, User) {

    if (!User.Verified) {
        $location.path("/user");
    }

    $scope.groups = _.where(Team.PlayerGroups, function(group) { return group.Players.length > 0; });
    $scope.sending = false;
    $scope.message = {};

    $scope.recipients = function () {
        var str = [];
        _(Team.PlayerGroups).flatten("Players").select(function (member) { return member.Selected && member.Email; }).each(function (member) {
            str.push(member.Email + "; ");
        });
        return str.join('');
    };
    
    $scope.toggleGroup = function (group) {
        group.Selected = !group.Selected;
        
        _.each(group.Players, function(member) {
            member.Selected = group.Selected;
        });
    };

    // Availability
    $scope.availabilityEvents = _(Team.Seasons).flatten("Games")
        .select(function (event) {
            return event.DateTime && moment(event.DateTime).isAfter(moment());
        })
        .sortBy(function (event) {
            return new Date(event.DateTime);
        }).value();

    $scope.selectedEvent = _.find($scope.availabilityEvents, function (event) { return event.Id == $routeParams.event; });
    $scope.requestAvailability = true;
    if (!$scope.selectedEvent) {
        $scope.selectedEvent = $scope.availabilityEvents[0] || null;
        $scope.requestAvailability = false;
    }

    $scope.sendMessage = function () {
        
        var recipients = _(Team.PlayerGroups).flatten("Players").select(function (member) { return member.Selected && member.Email; }).pluck("Id").value();
        console.log(recipients);

        /*if (recipients.length == 0) {
            return $("#alert-modal").fadeAlert("show", "Message must have at least one recipient");
        }
        if (!$scope.subject || $scope.subject.trim().length == 0) {
            return $("#alert-modal").fadeAlert("show", "Please include a message subject");
        }
        if (!$scope.content || $scope.content.length == 0) {
            return $("#alert-modal").fadeAlert("show", "Please include content for your message");
        }*/

        $scope.sending = true;

        /*$http.post(Team.uri + "/messages", {
            recipientPlayerIds: recipients,
            subject: $scope.message.subject,
            content: $scope.message.content
        })
        .success(function(message) {

            Team.Messages.push(message);

            _.each(Team.PlayerGroups, function (group) { group.Selected = false; });
            _(Team.PlayerGroups).flatten("Players").each(function (member) { member.Selected = false; });
            $scope.message = {};
            $scope.sending = false;

            //$("#alert-modal").fadeAlert("show", "Message sent successfully", "alert-success");
            

            $location.path("/messages");
        })
        .error(function(errorMessage) {
            //$("#alert-modal").fadeAlert("show", JSON.parse(errorMessage), "alert-error"); // TODO should go in directive or view?
            $scope.requesting = false;
        });*/
    };

    $scope.$watch("selectedEvent", function (value, oldValue) {
        if(angular.equals(value, oldValue)) return;
        $scope.requestAvailability = true;
    });

});