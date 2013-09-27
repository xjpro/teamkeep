angular.module("teamkeep").controller("ScheduleController", function ($scope, $filter, Team, GroupedTableDecorator, SortingDecorator) {

    GroupedTableDecorator.decorate($scope, {
        editPageUrl: "/schedule/events/",
        collectionName: "Seasons",
        collectionDisplayName: "Season",
        itemName: "Games",
        itemCollectionId: "SeasonId"
    });
    SortingDecorator.decorate($scope, "DateTime");
    
    $scope.seasons = Team.Seasons;
    $scope.members = _.flatten(Team.PlayerGroups, "Players");
    $scope.memberName = function (memberId) {
        var member = _.find($scope.members, function (member) { return member.Id == memberId; });
        return member ? $filter("playerName")(member) : null;
    };

    $scope.addEventDuty = Team.Seasons.addEventDuty;
    $scope.removeEventDuty = Team.Seasons.removeEventDuty;
    $scope.toggleDuties = function (event) {
        event.ShowDuties = !event.ShowDuties;
        _(Team.Seasons).flatten("Games").reject(function (other) { return !other.ShowDuties || other.Id == event.Id; }).each(function (event) {
            event.ShowDuties = false;
        });
    };
});