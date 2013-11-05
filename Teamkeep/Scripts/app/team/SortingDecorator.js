angular.module("teamkeep").service("SortingDecorator", ["$filter", "Team", function ($filter, Team) {

    this.decorate = function ($scope, startingSort) {
        $scope.sortType = startingSort;
        $scope.sortReverse = false;

        $scope.sortPredicate = function (item) {
            // Schedule sorts
            if ($scope.sortType == "DateTime") {
                return (item.DateTime) ? new Date(item.DateTime) : new Date(0);
            }
            else if ($scope.sortType == "Results") {
                return $filter("eventResults")(item, Team.ResultsView) || "";
            }
            else if ($scope.sortType == "OpponentName") {
                return item.OpponentName || "zzz";
            }
            else if ($scope.sortType == "Location") {
                return $filter("eventLocation")(item) || "zzz";
            }
            // Roster sorts
            else if ($scope.sortType == "Name") {
                return $filter("playerName")(item) || "zzz";
            }
            else if ($scope.sortType == "Position") {
                return item.Position || "zzz";
            }
            else if ($scope.sortType == "Contact") {
                return $filter("playerContact")(item) || "zzz";
            }

            return item[$scope.sortType] || "zzz";
        };

        $scope.sortBy = function (type) {
            if ($scope.sortType == type) {
                $scope.sortReverse = !$scope.sortReverse;
            } else {
                $scope.sortType = type;
                $scope.sortReverse = (type == "Results") ? true : false;
            }
        };

        $scope.$watch("[sortType, sortReverse]", function () {
            _.each(Team.Seasons, function (season) {
                season.Games = $filter("orderBy")(season.Games, $scope.sortPredicate, $scope.sortReverse);
            });
            _.each(Team.PlayerGroups, function (group) {
                group.Players = $filter("orderBy")(group.Players, $scope.sortPredicate, $scope.sortReverse);
            });
            $scope.sortIcon = ($scope.sortReverse) ? "fa fa-caret-up" : "fa fa-caret-down";
        }, true);
    };

}]);