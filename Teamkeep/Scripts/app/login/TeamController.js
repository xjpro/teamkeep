angular.module("teamkeep-public").controller("TeamController", ["$scope", function ($scope) {
    $scope.teamType = "sports";
    $scope.teamPublic = false;
    $scope.teamPopulate = false;
    $scope.createTeam = function() {
        console.log($scope.teamType, $scope.teamName, $scope.teamPublic, $scope.teamPopulate);
    };
}]);