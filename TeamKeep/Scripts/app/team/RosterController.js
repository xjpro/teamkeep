angular.module("teamkeep").controller("RosterController", ["$scope", "$filter", "Team", function ($scope, $filter, Team) {

    $scope.editable = Team.Editable;
    $scope.updating = Team.updating;
    $scope.groups = Team.PlayerGroups;
    
    $scope.selectGroup = function(group) {
        $scope.selectedGroup = group;
    };

    $scope.changeGroupOrder = function (group, desiredOrder) {

        if (desiredOrder < 0 || desiredOrder >= Team.PlayerGroups.length) return;

        var swapping = _.find(Team.PlayerGroups, function (otherGroup) { return otherGroup.Order == desiredOrder; });
        swapping.Order = group.Order;
        group.Order = desiredOrder;
    };

    $scope.removeGroup = function (group) {
        Team.removeGroup(group);
    };

    $scope.addPlayer = function (toGroup) {
        if (toGroup === -1) {
            Team.addPlayer(_.max(Team.PlayerGroups, 'Order').Id)
                .success(function () {
                    _.defer(function () { $("#roster tr:last input:first").focus(); });
                });
        }
        else if (!toGroup) {
            Team.addGroup("Players")
                .success(function () {
                    Team.addPlayer(_.last(Team.PlayerGroups).Id)
                        .success(function () {
                            _.defer(function () { $("#roster tr:last input:first").focus(); });
                        });
                });
        } else {
            Team.addPlayer(toGroup.Id)
                .success(function () {
                    _.defer(function () { $("#roster tbody[group-id='" + toGroup.Id + "'] tr:last input:first").focus(); });
                });
        }
    };
    
    $scope.removePlayer = function(player) {
        Team.removePlayer(player);
    };

    $scope.move = function(player, group) {
        var currentParent = _.find(Team.PlayerGroups, function(other) { return other.Id == player.GroupId; });
        currentParent.Players.splice(_.findIndex(currentParent.Players, function(other) { return other.Id == player.Id; }), 1);

        if (!group) {
            Team.addGroup("Players")
                .success(function() {
                    var added = _.last(Team.PlayerGroups);
                    player.GroupId = added.Id;
                    added.Players.push(player);
                });
        } else {
            player.GroupId = group.Id;
            group.Players.push(player);
        }
    };

    // Sorting
    $scope.preventSorting = false;
    $scope.sortType = "LastName";
    $scope.sortBy = function(newPredicate) {
        if (newPredicate == $scope.sortType) {
            $scope.reverse = !$scope.reverse;
        } else {
            $scope.sortType = newPredicate;
            $scope.reverse = false;
        }
    };
    $scope.reverse = false;
    $scope.predicate = function (item) {
        return item[$scope.sortType] || "zzz";
    };
    $scope.$watch("[preventSorting, sortType, reverse]", function () {
        if (!$scope.preventSorting) {
            for (var i = 0; i < $scope.groups.length; i++) {
                $scope.groups[i].Players = $filter("orderBy")($scope.groups[i].Players, $scope.predicate, $scope.reverse);
            }
        }
    }, true);

    // Column settings   

    if (typeof localStorage["Column.Position"] === "undefined") localStorage["Column.Position"] = true;
    if (typeof localStorage["Column.Phone"] === "undefined") localStorage["Column.Phone"] = true;
    if (typeof localStorage["Column.Email"] === "undefined") localStorage["Column.Email"] = true;

    $scope.columns = [
        {
            cssClass: "button",
            name: "",
            visible: Team.Editable,
            toggleable: false,
            sortType: null
        },
        {
            cssClass: "sort-lastname lastname",
            name: "Last name",
            toolTip: "Team member last name",
            visible: Team.Settings.LastNameColumn,
            toggleable: false,
            sortType: "LastName"
        },
        {
            cssClass: "sort-firstname max",
            name: Team.Settings.LastNameColumn ? "First name" : "Name",
            toolTip: "Team member first name",
            visible: true,
            toggleable: false,
            sortType: "FirstName"
        },
        {
            cssClass: "sort-position position",
            name: "Position",
            toolTip: "Team member position",
            visible: Team.Settings.PositionColumn && localStorage["Column.Position"] == "true",
            toggleable: Team.Settings.PositionColumn,
            sortType: "Position"
        },
        {
            cssClass: "sort-phone phone",
            name: "Phone",
            toolTip: "Primary contact phone number",
            visible: Team.Editable && Team.Settings.PhoneColumn && localStorage["Column.Phone"] == "true",
            toggleable: Team.Settings.PhoneColumn,
            sortType: "Phone"
        },
        {
            cssClass: "sort-email email",
            name: "Email",
            toolTip: "Email address",
            visible: Team.Editable && Team.Settings.EmailColumn && localStorage["Column.Email"] == "true",
            toggleable: Team.Settings.EmailColumn,
            sortType: "Email"
        }
    ];
    
    $scope.hasToggleableColumn = function () {
        return _.find($scope.columns, function (column) { return column.toggleable; });
    };
    
    $scope.$watch("columns", function () {
        angular.forEach($scope.columns, function (column) {
            if (column.toggleable) {
                localStorage["Column." + column.sortType] = column.visible;
            }
        });
    }, true);

    $scope.$watch(function () { return Team.Settings.LastNameColumn; }, function (value) {
        $scope.columns[1].visible = value;
        if (value) {
            $scope.columns[2].tooltip = "Team member first name";
            $scope.columns[2].name = "First name";
            $("body").append("<style>@@media (max-width: 767px) { #roster td:nth-of-type(3):before { content: 'First name'; } }</style>");
        }
        else {
            $scope.columns[2].tooltip = "Team member name";
            $scope.columns[2].name = "Name";
            $("body").append("<style>@@media (max-width: 767px) { #roster td:nth-of-type(3):before { content: 'Name'; } }</style>");
        }

    }, true);

    $scope.$watch(function () { return Team.Settings.PositionColumn; }, function (value, oldValue) {
        if (angular.equals(value, oldValue)) return;
        $scope.columns[3].visible = value;
        $scope.columns[3].toggleable = value;
    }, true);

    $scope.$watch(function () { return Team.Settings.PhoneColumn; }, function (value, oldValue) {
        if (angular.equals(value, oldValue)) return;
        $scope.columns[4].visible = value;
        $scope.columns[4].toggleable = value;
    }, true);
    
    $scope.$watch(function () { return Team.Settings.EmailColumn; }, function (value, oldValue) {
        if (angular.equals(value, oldValue)) return;
        $scope.columns[5].visible = value;
        $scope.columns[5].toggleable = value;
    }, true);
    
}]);