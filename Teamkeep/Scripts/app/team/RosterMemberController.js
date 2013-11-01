angular.module("teamkeep").controller("RosterMemberController", function($scope, $routeParams, $location, Team) {
    $scope.member = _.find(_.flatten(Team.PlayerGroups, "Players"), function (member) { return member.Id == $routeParams.memberId; });
    $scope.editable = Team.Editable;
    $scope.settings = Team.Settings;
    $scope.removeMember = function (member) {
        Team.PlayerGroups.removeItem(member);
        $location.path("/roster");
    };
    $scope.$watch("isMobile", function (isMobile) {
        if (!isMobile) {
            $location.path("/roster");
        }
    });
});