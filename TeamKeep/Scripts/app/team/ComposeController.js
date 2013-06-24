angular.module("teamkeep").controller("ComposeController", ["$scope", "$http", "$location", "Team", function ($scope, $http, $location, Team) {

    $scope.groups = Team.PlayerGroups;
    $scope.sending = false;

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

    $scope.sendMessage = function() {

        var recipients = _.pluck($scope.selectedMembers(), 'Id');

        if (recipients.length == 0) {
            return $("#alert-modal").fadeAlert("show", "Message must have at least one recipient");
        }
        if (!$scope.subject || $scope.subject.trim().length == 0) {
            return $("#alert-modal").fadeAlert("show", "Please include a message subject");
        }
        if (!$scope.content || $scope.content.length == 0) {
            return $("#alert-modal").fadeAlert("show", "Please include content for your message");
        }

        $scope.sending = true;

        $http.post(Team.uri + "/messages", {
            recipientPlayerIds: recipients,
            subject: $scope.subject,
            content: $scope.content
        })
        .success(function(message) {

            Team.Messages.push(message);

            _.each(_.flatten(Team.PlayerGroups, "Players"), function (member) { member.selected = false; }); // Remove selections
            $scope.subject = "";
            $scope.content = "";
            
            $("#alert-modal").fadeAlert("show", "Message sent successfully", "alert-success");
            $scope.sending = false;

            $location.path("/messages");
        })
        .error(function(errorMessage) {
            $("#alert-modal").fadeAlert("show", JSON.parse(errorMessage), "alert-error"); // TODO should go in directive or view?
            $scope.requesting = false;
        });
    };

}]);