angular.module("teamkeep").factory("Team", ["$rootScope", "$http", "$timeout", function($rootScope, $http, $timeout) {

    var Team = (window.viewData && window.viewData.Team) ? window.viewData.Team : {
        Id: 0,
        Settings: {}
    };

    Team.uri = "/teams/" + Team.Id + "/" + Team.Name;
    Team.updating = false;

    Team.saveSettings = function () {
        return $http.put(Team.uri + "/settings", {
            teamId: Team.Id,
            name: Team.Name,
            privacy: Team.Privacy,
            settings: Team.Settings
        }).success(function (allSettings) {
            // The below *should* be unnecessary I think
            //Team.Privacy = allSettings.Privacy;
            //Team.Settings = allSettings.Settings;
        });
    };

    Team.addSeason = function(name) {
        Team.updating = true;
        
        return $http.post(Team.uri + "/seasons", { name: name, order: Team.Seasons.length })
            .success(function (season) {
                Team.Seasons.push(season);
                $rootScope.$watch(function () { return season.Name + season.Order; }, function (value, oldValue) { queueChange("seasons", season, value, oldValue); }, true);

                Team.updating = false;
            });
    };

    Team.removeSeason = function (season) {
        Team.updating = true;

        return $http.delete(Team.uri + "/seasons/" + season.Id, { id: season.Id })
            .success(function () {
                Team.Seasons.splice(_.findIndex(Team.Seasons, function (otherSeason) { return otherSeason.Id == season.Id; }), 1);
                _.each(Team.Seasons, function (other, index) { other.Order = index; });
                Team.updating = false;
            });
    };

    Team.addEvent = function (toSeasonId) {
        Team.updating = true;

        return $http.post("/games", { homeTeamId: Team.Id, seasonId: toSeasonId })
            .success(function (event) {
                var parentSeason = _.find(Team.Seasons, function (season) { return season.Id == toSeasonId; });
                parentSeason.Games.push(event);
                $rootScope.$watch(function () { return event; }, function (value, oldValue) { queueChange("events", event, value, oldValue); }, true);
            
                Team.updating = false;
            });
    };
    
    Team.removeEvent = function (event) {
        Team.updating = true;

        return $http.delete("/games/" + event.Id)
            .success(function() {
                var parentSeason = _.find(Team.Seasons, function (season) { return season.Id == event.SeasonId; });
                parentSeason.Games.splice(_.findIndex(parentSeason.Games, function(otherEvent) { return otherEvent.Id == event.Id; }), 1);

                Team.updating = false;
            });
    };

    Team.addGroup = function(name) {
        return $http.post(Team.uri + "/groups", { name: name, order: Team.PlayerGroups.length })
            .success(function (group) {
                Team.PlayerGroups.push(group);
                $rootScope.$watch(function () { return group.Name + group.Order; }, function (value, oldValue) { queueChange("groups", group, value, oldValue); }, true);

                Team.updating = false;
            });
    };
    
    Team.removeGroup = function (group) {
        Team.updating = true;

        return $http.delete(Team.uri + "/groups/" + group.Id, { id: group.Id })
            .success(function () {
                Team.PlayerGroups.splice(_.findIndex(Team.PlayerGroups, function (otherGroup) { return otherGroup.Id == group.Id; }), 1);
                _.each(Team.PlayerGroups, function (other, index) { other.Order = index; });
                Team.updating = false;
            });
    };

    Team.addPlayer = function (parentId) {
        Team.updating = true;

        return $http.post(Team.uri + "/players", { groupId: parentId })
            .success(function (player) {
                var parent = _.find(Team.PlayerGroups, function (candidate) { return candidate.Id == parentId; });
                parent.Players.push(player);
                $rootScope.$watch(function () { return player; }, function (value, oldValue) { queueChange("players", player, value, oldValue); }, true);

                Team.updating = false;
            });
    };

    Team.removePlayer = function (player) {
        Team.updating = true;

        return $http.delete(Team.uri + "/players/" + player.Id)
            .success(function () {
                var parent = _.find(Team.PlayerGroups, function (candidate) { return candidate.Id == player.GroupId; });
                parent.Players.splice(_.findIndex(parent.Players, function (other) { return other.Id == player.Id; }), 1);

                Team.updating = false;
            });
    };


    // Change tracking

    var timeout = null;
    var changes = {
        
        seasons: {},
        events: {},
        groups: {},
        players: {},
        
        clear: function () {
            this.events = {};
            this.seasons = {};
            this.groups = {};
            this.players = {};
        }
    };
    
    var uploadChanges = function () {
        Team.updating = true;
        
        $http.put(Team.uri, {
            seasons: _.toArray(changes.seasons),
            events: _.toArray(changes.events),
            playerGroups: _.toArray(changes.groups),
            players: _.toArray(changes.players)
        }).then(function () {
            Team.updating = false;
        });
        changes.clear();
    };

    var queueChange = function (type, item, value, oldValue) {
        if (angular.equals(value, oldValue)) return;

        changes[type][item.Id] = item;

        $timeout.cancel(timeout);
        timeout = $timeout(uploadChanges, 1000);
    };

    // Setup watches
    angular.forEach(Team.Seasons, function (season) {
        $rootScope.$watch(function () { return season.Name + season.Order; }, function (value, oldValue) { queueChange("seasons", season, value, oldValue); }, true);
        angular.forEach(season.Games, function (event) {
            $rootScope.$watch(function () { return event; }, function (value, oldValue) { queueChange("events", event, value, oldValue); }, true);
        });
    });
    angular.forEach(Team.PlayerGroups, function (group) {
        $rootScope.$watch(function () { return group.Name + group.Order; }, function (value, oldValue) { queueChange("groups", group, value, oldValue); }, true);
        angular.forEach(group.Players, function (player) {
            $rootScope.$watch(function () { return player; }, function (value, oldValue) { queueChange("players", player, value, oldValue); }, true);
        });
    });

    return Team;
}]);