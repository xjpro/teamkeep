angular.module("teamkeep").controller("HeaderController", function ($scope, $location, Team, User) {
    $scope.teams = User.Teams;
    $scope.sidebarActive = false;
    $scope.title = "Schedule";
    $scope.name = function () { return Team.Name };
    $scope.bannerImage = "/TeamBanners/" + Team.BannerImage;

    $scope.goto = function (path) {
        if ($scope.isMobile) {
            $scope.sidebarActive = false;
        }
        $location.path(path);
    };
    $scope.logout = function () {
        User.logout();
    };

    $scope.$watch("isMobile", function(value) {
        $scope.sidebarActive = !value;
    }, true);
    $scope.$on("$toggleSidebar", function() {
        $scope.sidebarActive = !$scope.sidebarActive;
    });
});