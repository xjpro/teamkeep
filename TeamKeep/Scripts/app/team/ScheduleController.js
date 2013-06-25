angular.module("teamkeep").controller("ScheduleController", ["$scope", "$filter", "Team", function ($scope, $filter, Team) {

    $scope.isMobile = TeamKeep.isMobile;
    $scope.editable = Team.Editable;
    $scope.updating = function () { return Team.updating; };
    $scope.seasons = Team.Seasons;
    $scope.players = Team.playersWithName();

    $scope.eventTypeIcon = Team.eventTypeIcon;

    $scope.eventTypeTooltip = function (eventType) {
        switch (eventType) {
            case 0: return "Game"; // vs.
            case 1: return "Practice"; // practice
            case 2: return "Meeting"; // meeting
            case 3: return "Celebration"; // meeting
            default: return "None"; // blank
        }
    };

    $scope.locations = _(Team.Seasons).flatten("Games").flatten("Location");

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

    $scope.showLocationEditor = function (location) {
        var inputCount = 0;
        if (location.Description) inputCount++;
        if (location.Street) inputCount++;
        if (location.City) inputCount++;
        if (location.Postal) inputCount++;
        return inputCount > 1;
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
            Team.addEvent(_.max(Team.Seasons, 'Order').Id)
                .success(function() {
                    _.defer(function() { $("#schedule tr:last input:first").focus(); });
                });
        }
        else if (!toSeason) {
            Team.addSeason("Untitled Season")
                .success(function () {
                    Team.addEvent(_.last(Team.Seasons).Id)
                        .success(function () {
                            _.defer(function () { $("#schedule tr:last input:first").focus(); });
                        });
                });
        } else {
            Team.addEvent(toSeason.Id)
                .success(function () {
                    _.defer(function () { $("#schedule tbody[season-id='" + toSeason.Id + "'] tr:last input:first").focus(); });
                });
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

    $scope.rotateType = function (event) {
        if (!$scope.editable) return;
        if (++event.Type > 4) {
            event.Type = 0;
        } 
    };
    
    // Duties

    $scope.dutyName = "";
    $scope.dutyPlayerId = $scope.players[0].Id;
    
    $scope.selectDutyEvent = function (event) {
        $scope.dutyEvent = event;
    };

    $scope.addDuty = function () {
        console.log($scope.dutyName);
        Team.addEventDuty($scope.dutyEvent, $scope.dutyPlayerId, $scope.dutyName)
            .success(function () {
                // TODO controller shouldn't be handling view stuff
                $("#alert-modal").fadeAlert("show", "'" + $scope.dutyName + "' duty was successfully added", "alert-success");
                $("#duty-modal").modal("hide");
            });
    };

    // Sorting
    $scope.preventSorting = false;
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
            return $scope.locationDisplay(item.Location) || "zzz";
        }
        else if ($scope.sortType == "SubLocation") {
            return (item.Location) ? item.Location.InternalLocation || "zzz" : "zzz";
        }
        return item[$scope.sortType] || "zzz";
    };
    $scope.$watch("[preventSorting, sortType, reverse]", function () {
        if (!$scope.preventSorting) {
            for (var i = 0; i < $scope.seasons.length; i++) {
                $scope.seasons[i].Games = $filter("orderBy")($scope.seasons[i].Games, $scope.predicate, $scope.reverse);
            }
        }
    }, true);

    // Column settings

    if (typeof localStorage["Column.DateTime"] === "undefined") localStorage["Column.DateTime"] = true;
    if (typeof localStorage["Column.ScoredPoints"] === "undefined") localStorage["Column.ScoredPoints"] = true;
    if (typeof localStorage["Column.AllowedPoints"] === "undefined") localStorage["Column.AllowedPoints"] = true;
    if (typeof localStorage["Column.TiePoints"] === "undefined") localStorage["Column.TiePoints"] = true;
    if (typeof localStorage["Column.Location"] === "undefined") localStorage["Column.Location"] = true;
    if (typeof localStorage["Column.SubLocation"] === "undefined") localStorage["Column.SubLocation"] = true;

    $scope.columns = [
        {
            cssClass: "button",
            name: "",
            visible: Team.Editable,
            toggleable: false
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
            cssClass: "icon",
            name: "",
            visible: true,
            toggleable: false
        },
        {
            cssClass: "sort-opponent max",
            name: "Title",
            toolTip: "A short description or title of the event",
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
    
    $scope.$watch("columns", function () {
        angular.forEach($scope.columns, function (column) {
            if (column.toggleable) {
                localStorage["Column." + column.sortType] = column.visible;
            }
        });
    }, true);

    // Assign results view values
    (function () {
        switch (Team.Settings.ResultsView) {
            case 0: // points scored / allowed
                $scope.columns[2].name = "PS";
                $scope.columns[2].toolTip = "Points scored";
                $scope.columns[2].visible = localStorage["Column.ScoredPoints"] == "true";
                $scope.columns[2].toggleable = true;
                $scope.columns[2].toggleName = "Points scored";

                $scope.columns[3].name = "PA";
                $scope.columns[3].toolTip = "Points allowed";
                $scope.columns[3].visible = localStorage["Column.AllowedPoints"] == "true";
                $scope.columns[3].toggleable = true;
                $scope.columns[3].toggleName = "Points allowed";

                $scope.columns[4].visible = false;
                $scope.columns[4].toggleable = false;

                $("body").append("<style>@@media (max-width: 767px) { " +
                    "#schedule td:nth-of-type(3):before { content: 'Scored'; } " +
                    "#schedule td:nth-of-type(4):before { content: 'Allowed'; } }</style>");
                break;
            case 1: // Best of
                $scope.columns[2].name = "W";
                $scope.columns[2].toolTip = "Games won";
                $scope.columns[2].visible = localStorage["Column.ScoredPoints"] == "true";
                $scope.columns[2].toggleable = true;
                $scope.columns[2].toggleName = "Wins";

                $scope.columns[3].name = "L";
                $scope.columns[3].toolTip = "Games lost";
                $scope.columns[3].visible = localStorage["Column.AllowedPoints"] == "true";
                $scope.columns[3].toggleable = true;
                $scope.columns[3].toggleName = "Losses";

                $scope.columns[4].name = "T";
                $scope.columns[4].toolTip = "Games tied";
                $scope.columns[4].visible = localStorage["Column.TiePoints"] == "true";
                $scope.columns[4].toggleable = true;
                $scope.columns[4].toggleName = "Ties";

                $("body").append("<style>@@media (max-width: 767px) { " +
                    "#schedule td:nth-of-type(3):before { content: 'Win'; } " +
                    "#schedule td:nth-of-type(4):before { content: 'Loss'; } }</style>");
                break;
            case 2: // Win/lose (binary result)
                $scope.columns[2].name = "W/L";
                $scope.columns[2].toolTip = "Final result (win/loss)";
                $scope.columns[2].visible = localStorage["Column.ScoredPoints"] == "true";
                $scope.columns[2].toggleable = true;
                $scope.columns[2].toggleName = "Result (W/L)";

                $scope.columns[3].visible = false;
                $scope.columns[3].toggleable = false;

                $scope.columns[4].visible = false;
                $scope.columns[4].toggleable = false;
                $("body").append("<style>@@media (max-width: 767px) { #schedule td:nth-of-type(3):before { content: 'Result'; } }</style>");
                break;
            case 3: // no results
                $scope.columns[2].visible = false;
                $scope.columns[2].toggleable = false;

                $scope.columns[3].visible = false;
                $scope.columns[3].toggleable = false;

                $scope.columns[4].visible = false;
                $scope.columns[4].toggleable = false;
                break;
        }
    })();

}]);