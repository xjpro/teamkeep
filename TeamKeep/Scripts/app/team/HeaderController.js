angular.module("teamkeep").controller("HeaderController", function ($scope, $location, Team, User) {
    $scope.teams = User.Teams;
    $scope.sidebarActive = false;
    $scope.createTeamModalActive = false;
    $scope.createTeamModalDismissable = true;
    $scope.title = "Schedule";

    $scope.id = Team.Id;
    $scope.name = function () { return Team.Name };
    $scope.bannerImage = "/TeamBanners/" + Team.BannerImage;

    $scope.goto = function (path) {
        if ($scope.isMobile) {
            $scope.sidebarActive = false;
        }
        $location.path(path);

        switch (path) {
            case 'messages': $scope.title = "Messaging"; break;
            default: $scope.title = path.charAt(0).toUpperCase() + path.slice(1); break;
        }
    };
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

    if (User.Teams.length == 0) {
        $scope.createTeamModalActive = true;
        $scope.createTeamModalDismissable = false;
    }
});