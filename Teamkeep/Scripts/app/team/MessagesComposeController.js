﻿angular.module("teamkeep").controller("MessagesComposeController", function ($scope, $http, $location, Team) {

    $scope.groups = _.where(Team.PlayerGroups, function(group) { return group.Players.length > 0; });
    $scope.sending = false;
    $scope.message = {};

    $scope.toggleGroup = function (group) {
        group.Selected = !group.Selected;
        
        _.each(group.Players, function(member) {
            member.Selected = group.Selected;
        });
    };

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

});