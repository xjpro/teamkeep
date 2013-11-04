angular.module("teamkeep").controller("RosterController", ["$scope", "Team", "GroupedTableDecorator", "SortingDecorator", function ($scope, Team, GroupedTableDecorator, SortingDecorator) {

    GroupedTableDecorator.decorate($scope, {
        editPageUrl: "/roster/members/",
        collectionName: "PlayerGroups",
        collectionDisplayName: "Group",
        itemName: "Players",
        itemCollectionId: "GroupId"
    });
    SortingDecorator.decorate($scope, "Name");
    
    $scope.editable = Team.Editable;
    $scope.groups = Team.PlayerGroups;
    $scope.settings = Team.Settings;
}]);