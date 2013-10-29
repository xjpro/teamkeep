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
    $scope.settings = Team.Settings;
    $scope.members = _.flatten(Team.PlayerGroups, "Players");
    $scope.memberName = function (memberId) {
        var member = _.find($scope.members, function (member) { return member.Id == memberId; });
        return member ? $filter("playerName")(member) : null;
    };
    $scope.eventTypes = [
        { name: "Game", value: 0 },
        { name: "Practice", value: 1 },
        { name: "Meeting", value: 2 },
        { name: "Party", value: 3 },
        { name: "None", value: 99 }
    ];

    $scope.addEventDuty = Team.Seasons.addEventDuty;
    $scope.removeEventDuty = Team.Seasons.removeEventDuty;
    $scope.toggleDuties = function (event) {
        event.ShowDuties = !event.ShowDuties;
        _(Team.Seasons).flatten("Games").reject(function (other) { return !other.ShowDuties || other.Id == event.Id; }).each(function (event) {
            event.ShowDuties = false;
        });
    };
    $scope.closeDuties = function () {
        $("body").click();
    };
});