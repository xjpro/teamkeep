angular.module("teamkeep").controller("HeaderController", function($scope, Team) {
    $scope.sidebarActive = false;
    $scope.title = "Schedule";
    $scope.name = function () { return Team.Name };
    $scope.bannerImage = "/TeamBanners/" + Team.BannerImage;

    $scope.$watch("isMobile", function(value) {
        $scope.sidebarActive = !value;
    }, true);
    $scope.$on("$toggleSidebar", function() {
        $scope.sidebarActive = !$scope.sidebarActive;
    });
});