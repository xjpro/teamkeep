angular.module("teamkeep").controller("ScheduleController", ["$scope", "Team", function ($scope, Team) {

    $scope.isMobile = TeamKeep.isMobile;
    $scope.editable = Team.Editable;
    $scope.updating = Team.updating;
    $scope.seasons = Team.Seasons;
    
    $scope.locationDisplay = function(location) {
        if (location != null) {
            if (location.Description) return location.Description;

            var address = [];
            address.push(location.Street || "");
            if (location.City) { address.push(" " + location.City); };
            if (location.Postal) { address.push(" " + location.Postal); };
            return address.join('').trim();
        }
        return "";
    };

    $scope.selectSeason = function(season) {
        $scope.selectedSeason = season;
    };

    $scope.changeSeasonOrder = function (season, desiredOrder) {

        if (desiredOrder < 0 || desiredOrder >= Team.Seasons.length) return;

        var swapping = _.find(Team.Seasons, function (otherSeason) { return otherSeason.Order == desiredOrder; });
        swapping.Order = season.Order;
        season.Order = desiredOrder;
    };

    $scope.removeSeason = function (season) {
        Team.removeSeason(season);
    };

    $scope.addEvent = function (toSeason) {
        if (toSeason === -1) {
            Team.addEvent(_.last(Team.Seasons).Id);
        }
        else if (!toSeason) {
            Team.addSeason("Untitled Season")
                .success(function () {
                    Team.addEvent(_.last(Team.Seasons).Id);
                });
        } else {
            Team.addEvent(toSeason.Id);
        }
    };
    $scope.removeEvent = function(event) {
        Team.removeEvent(event);
    };
    $scope.move = function(event, season) {
        var currentParent = _.find(Team.Seasons, function (other) { return other.Id == event.SeasonId; });
        currentParent.Games.splice(_.findIndex(currentParent.Games, function (other) { return other.Id == event.Id; }), 1);

        if (!season) {
            Team.addSeason("Untitled Season")
                .success(function () {
                    var added = _.last(Team.Seasons);
                    event.SeasonId = added.Id;
                    added.Games.push(event);
                });
        } else {
            event.SeasonId = season.Id;
            season.Games.push(event);
        }
    };

    // Sorting
    $scope.sortType = "DateTime";
    $scope.sortBy = function(newPredicate) {
        if (newPredicate == $scope.sortType) {
            $scope.reverse = !$scope.reverse;
        } else {
            $scope.sortType = newPredicate;

            if (newPredicate == "ScoredPoints" || newPredicate == "AllowedPoints" || newPredicate == "TiePoints") {
                $scope.reverse = true;
            } else {
                $scope.reverse = false;
            }
        }
    };
    $scope.reverse = false;
    $scope.predicate = function (item) {
        if ($scope.sortType == "DateTime") {
            return (item.DateTime) ? new Date(item.DateTime) : new Date(0);
        }
        else if ($scope.sortType == "ScoredPoints" || $scope.sortType == "AllowedPoints" || $scope.sortType == "TiePoints") {
            return item[$scope.sortType] || 0;
        }
        else if ($scope.sortType == "Location") {
            return $scope.locationDisplay(item.Location);
        }
        else if ($scope.sortType == "SubLocation") {
            return (item.Location) ? item.Location.InternalLocation : "";
        }
        return item[$scope.sortType] || "";
    };

    // Column settings
    $scope.$watch("columns", function() {
        angular.forEach($scope.columns, function(column) {
            if (column.toggleable) {
                localStorage["Column." + column.sortType] = column.visible;
            }
        });
    }, true);
    
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
            visible: localStorage["Column.DateTime"] == "true",
            toggleable: true,
            sortType: "DateTime"
        },
        {
            id: "sort-scoredpoints",
            cssClass: "sort-scoredpoints digit",
            name: "PS",
            toolTip: "Points scored by your team",
            visible: localStorage["Column.ScoredPoints"] == "true",
            toggleable: true,
            toggleName: "Points scored",
            sortType: "ScoredPoints"
        },
        {
            cssClass: "sort-allowedpoints digit",
            name: "PA",
            toolTip: "Points scored by opposing team",
            visible: localStorage["Column.AllowedPoints"] == "true",
            toggleable: true,
            toggleName: "Points scored",
            sortType: "AllowedPoints"
        },
        {
            cssClass: "sort-tiepoints digit",
            name: "PT",
            toolTip: "Points tie",
            visible: localStorage["Column.TiePoints"] == "true",
            toggleable: Team.Settings.ResultsView == 1,
            toggleName: "Points tie",
            sortType: "TiePoints"
        },
        {
            cssClass: "sort-opponent max",
            name: "Opponent",
            toolTip: "Name of opposing team",
            visible: true,
            toggleable: false,
            sortType: "OpponentName"
        },
        {
            cssClass: "sort-location location",
            name: "Location",
            toolTip: "Location where the event takes place",
            visible: localStorage["Column.Location"] == "true",
            toggleable: true,
            sortType: "Location"
        },
        {
            cssClass: "sort-sublocation sublocation",
            name: "Arena",
            toolTip: "Field, court, or rink",
            visible: Team.Settings.ArenaColumn && localStorage["Column.SubLocation"] == "true",
            toggleable: Team.Settings.ArenaColumn,
            sortType: "SubLocation"
        }
    ];

}]);