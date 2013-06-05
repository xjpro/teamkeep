angular.module("teamkeep").controller("ScheduleController", ["$scope", "Team", function ($scope, Team) {

    $scope.isMobile = TeamKeep.isMobile;
    $scope.editable = Team.Editable;
    
    $scope.seasons = Team.Seasons;
    $scope.locationDisplay = function(location) {
        
    };

    $scope.columns = [
        {
            cssClass: "button",
            name: "",
            visible: Team.Editable,
            toggleable: false,
            sortType: null
        },
        {
            cssClass: "sort-date date", 
            name: "Date",
            toolTip: "Date and time event occurs",
            visible: localStorage["column.dateTime.visible"] == "true",
            toggleable: true,
            sortType: "dateTime"
        },
        {
            id: "sort-scoredpoints",
            cssClass: "sort-scoredpoints digit",
            name: "PS",
            toolTip: "Points scored by your team",
            visible: true,
            toggleable: true,
            toggleName: "Points scored",
            SortType: "scoredPoints"
        },
        {
            cssClass: "sort-allowedpoints digit",
            name: "PA",
            toolTip: "Points scored by opposing team",
            visible: true,
            toggleable: true,
            toggleName: "Points scored",
            sortType: "allowedPoints"
        },
        {
            cssClass: "sort-tiepoints digit",
            name: "PT",
            toolTip: "Points tie",
            visible: true,
            toggleable: Team.Settings.ResultsView == 1,
            toggleName: "Points tie",
            sortType: "tiePoints"
        },
        {
            cssClass: "sort-opponent max",
            name: "Opponent",
            toolTip: "Name of opposing team",
            visible: true,
            toggleable: false,
            sortType: "opponentName"
        },
        {
            cssClass: "sort-location location",
            name: "Location",
            toolTip: "Location where the event takes place",
            visible: localStorage["column.location.visible"] == "true",
            toggleable: true,
            sortType: "location"
        },
        {
            cssClass: "sort-sublocation sublocation",
            name: "Arena",
            toolTip: "Field, court, or rink",
            visible: Team.Settings.ArenaColumn && localStorage["column.sublocation.visible"] == "true",
            toggleable: Team.Settings.ArenaColumn,
            sortType: "sublocation"
        }
    ];

}]);