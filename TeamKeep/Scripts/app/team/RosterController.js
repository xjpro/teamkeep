angular.module("teamkeep").controller("RosterController", ["$scope", "Team", function ($scope, Team) {

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
            Team.addPlayer(_.last(Team.PlayerGroups).Id);
        }
        else if (!toGroup) {
            Team.addGroup("Players")
                .success(function () {
                    Team.addPlayer(_.last(Team.PlayerGroups).Id);
                });
        } else {
            Team.addPlayer(toGroup.Id);
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

}]);