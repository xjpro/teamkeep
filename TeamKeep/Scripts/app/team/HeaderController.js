angular.module("teamkeep").controller("HeaderController", function($scope, Team) {
    $scope.sidebarActive = false;
    $scope.title = "Schedule";
    $scope.bannerImage = "/TeamBanners/" + Team.BannerImage;

    $scope.$watch("isMobile", function(value) {
        $scope.sidebarActive = !value;
    }, true);
    $scope.$on("$toggleSidebar", function() {
        $scope.sidebarActive = !$scope.sidebarActive;
    });
    $scope.$on("$routeChangeSuccess", function(evt, current, previous) {
        console.log(previous, current);
        switch (current.$$route.controller) {
            case "ScheduleController":
                $scope.title = "Schedule";
                break;
            case "EventController":
                $scope.title = "";
                break;
        }
    });
});