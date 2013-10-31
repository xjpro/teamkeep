angular.module("teamkeep").controller("HeaderController", function ($scope, $location, Team, User) {
    $scope.editable = Team.Editable;
    $scope.showRoster = Team.Privacy.Roster;
    $scope.loggedIn = User.Id != 0;

    $scope.teams = User.Teams;
    $scope.sidebarActive = false;
    $scope.createTeamModalActive = false;
    $scope.createTeamModalDismissable = true;

    $scope.id = Team.Id;
    $scope.name = function () { return Team.Name };
    $scope.bannerImage = "/TeamBanners/" + (Team.BannerImage || "defaultbanner.jpg");

    $scope.userTitle = User.Username || User.Email;
    $scope.userVerified = User.Verified;

    $scope.logout = function () {
        User.logout();
    };

    $scope.$watch("sidebarActive", function (value) {
        console.log("sidebarActive is now " + value);
    }, true);
    $scope.$watch("isMobile", function(value) {
        $scope.sidebarActive = !value;
    }, true);
    $scope.$on("$toggleSidebar", function() {
        $scope.sidebarActive = !$scope.sidebarActive;
    });
    $scope.$on("$routeChangeSuccess", function (ngEvent, next) {

        if (!next || !next.$$route) return;

        if ($scope.isMobile) {
            $scope.sidebarActive = false;
        }

        var matches = /\/([^\/]*)/.exec(next.$$route.originalPath);
        var firstPath = matches[1];

        switch (firstPath) {
            case "messages": $scope.title = "Messaging"; break;
            case "user": $scope.title = "User Settings"; break;
            default: $scope.title = firstPath.charAt(0).toUpperCase() + firstPath.slice(1); break;
        }
    });

    if ($scope.isOwner && User.Teams.length == 0) {
        $scope.createTeamModalActive = true;
        $scope.createTeamModalDismissable = false;
    }
    
});