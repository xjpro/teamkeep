angular.module("teamkeep")
    .factory("Team", function ($rootScope, $http, $timeout) {

        if (!window.viewData.Team) {
            window.viewData.Team = {
                Name: "",
                Seasons: [],
                PlayerGroups: []
            };
        }

        var Team = window.viewData.Team;
        
        // Split date and time
        _.each(_.flatten(Team.Seasons, "Games"), function (game) {
            if (game.DateTime) {
                game.DateTime = moment(game.DateTime).toDate();
            }
        });

        Team.uri = "/teams/" + Team.Id + "/" + Team.Name.replace(/\s+/g, "");
        Team.updating = false;

        Team.Seasons.addCollection = function (name) {
            Team.updating = true;

            return $http.post(Team.uri + "/seasons", { name: name, order: Team.Seasons.length })
                .success(function (season) {
                    Team.Seasons.push(season);
                    $rootScope.$watch(function () { return season.Name + season.Order; }, function (value, oldValue) { queueChange("seasons", season, value, oldValue); }, true);

                    Team.updating = false;
                });
        };

        Team.Seasons.removeCollection = function (season) {
            Team.updating = true;

            return $http.delete(Team.uri + "/seasons/" + season.Id, { id: season.Id })
                .success(function () {
                    Team.Seasons.splice(_.findIndex(Team.Seasons, function (otherSeason) { return otherSeason.Id == season.Id; }), 1);
                    _.each(Team.Seasons, function (other, index) { other.Order = index; });
                    Team.updating = false;
                });
        };

        Team.Seasons.addItem = function (toSeasonId) {
            Team.updating = true;

            return $http.post("/games", { homeTeamId: Team.Id, seasonId: toSeasonId })
                .success(function (event) {
                    var parentSeason = _.find(Team.Seasons, function (season) { return season.Id == toSeasonId; });
                    parentSeason.Games.push(event);
                    $rootScope.$watch(function () { return event; }, function (value, oldValue) { queueChange("events", event, value, oldValue); }, true);

                    $rootScope.$broadcast("teamkeep.newevent");
                    Team.updating = false;
                });
        };

        Team.Seasons.removeItem = function (event) {
            event.ShowDuties = false;
            Team.updating = true;    

            return $http.delete("/games/" + event.Id)
                .success(function () {
                    var parentSeason = _.find(Team.Seasons, function (season) { return season.Id == event.SeasonId; });
                    parentSeason.Games.splice(_.findIndex(parentSeason.Games, function (otherEvent) { return otherEvent.Id == event.Id; }), 1);

                    Team.updating = false;
                });
        };

        Team.Seasons.addEventDuty = function (event) {
            Team.updating = true;

            return $http.post(Team.uri + "/events/" + event.Id + "/duties", {
            }).success(function (duty) {
                event.Duties.push(duty);
                Team.updating = false;
            });
        };

        Team.Seasons.removeEventDuty = function (duty) {
            Team.updating = true;

            return $http.delete(Team.uri + "/events/" + duty.EventId + "/duties/" + duty.Id)
                .success(function () {
                    var parent = _.find(_.flatten(Team.Seasons, "Games"), function (event) { return event.Id == duty.EventId; });
                    parent.Duties.splice(_.findIndex(parent.Duties, function (other) { return other.Id == duty.Id; }), 1);
                    Team.updating = false;
                });
        };

        Team.PlayerGroups.addCollection = function (name) {
            return $http.post(Team.uri + "/groups", { name: name, order: Team.PlayerGroups.length })
                .success(function (group) {
                    Team.PlayerGroups.push(group);
                    $rootScope.$watch(function () { return group.Name + group.Order; }, function (value, oldValue) { queueChange("groups", group, value, oldValue); }, true);

                    Team.updating = false;
                });
        };

        Team.PlayerGroups.removeCollection = function (group) {
            Team.updating = true;

            return $http.delete(Team.uri + "/groups/" + group.Id, { id: group.Id })
                .success(function () {
                    Team.PlayerGroups.splice(_.findIndex(Team.PlayerGroups, function (otherGroup) { return otherGroup.Id == group.Id; }), 1);
                    _.each(Team.PlayerGroups, function (other, index) { other.Order = index; });
                    Team.updating = false;
                });
        };

        Team.PlayerGroups.addItem = function (parentId) {
            Team.updating = true;

            return $http.post(Team.uri + "/players", { groupId: parentId })
                .success(function (player) {
                    var parent = _.find(Team.PlayerGroups, function (candidate) { return candidate.Id == parentId; });
                    parent.Players.push(player);
                    $rootScope.$watch(function () { return player.LastName + player.FirstName + player.Position + player.Phone + player.Email + _.flatten(player.Availability, "AdminStatus"); },
                        function (value, oldValue) { queueChange("players", player, value, oldValue); }, true);

                    $rootScope.$broadcast("teamkeep.newplayer");
                    Team.updating = false;
                });
        };

        Team.PlayerGroups.removeItem = function (player) {
            Team.updating = true;

            return $http.delete(Team.uri + "/players/" + player.Id)
                .success(function () {
                    var parent = _.find(Team.PlayerGroups, function (candidate) { return candidate.Id == player.GroupId; });
                    parent.Players.splice(_.findIndex(parent.Players, function (other) { return other.Id == player.Id; }), 1);

                    // Reassign duties
                    _(Team.Seasons).flatten("Games").flatten("Duties").filter(function (duty) { return duty.PlayerId == 87; }).each(function (duty) {
                        duty.PlayerId = null;
                    });

                    Team.updating = false;
                });
        };
        
        Team.saveSettings = function () {
            return $http.put(Team.uri + "/settings", {
                teamId: Team.Id,
                name: Team.Name,
                privacy: Team.Privacy,
                settings: Team.Settings
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
            timeout = $timeout(uploadChanges, 100); // Set this to above 0 to delay changes
        };


        // Setup watches
        angular.forEach(Team.Seasons, function (season) {
            $rootScope.$watch(function () { return season.Name + season.Order; }, function (value, oldValue) { queueChange("seasons", season, value, oldValue); }, true);
            angular.forEach(season.Games, function (event) {
                $rootScope.$watch(function () { return _.omit(event, "Selected"); },
                    function (value, oldValue) { queueChange("events", event, value, oldValue); }, true);
            });
        });
        angular.forEach(Team.PlayerGroups, function (group) {
            $rootScope.$watch(function () { return group.Name + group.Order; }, function (value, oldValue) { queueChange("groups", group, value, oldValue); }, true);
            angular.forEach(group.Players, function (player) {
                $rootScope.$watch(function () { return _.pick(player, "GroupId", "LastName", "FirstName", "Position", "Phone", "Email") + _.flatten(player.Availability, "AdminStatus"); },
                    function (value, oldValue) { queueChange("players", player, value, oldValue); }, true);
            });
        });
        $rootScope.$watch(function () { return Team.Name + _.map(Team.Privacy) + _.map(Team.Settings); }, function (value, oldValue) {
            if (angular.equals(value, oldValue)) return;
            Team.saveSettings(); // TODO use changes endpoint
        }, true);

        return Team;
    })
    .filter("eventDate", function () {
        return function (event) {
            if (!event.DateTime) return null;
            var datetime = moment(event.DateTime);
            return datetime.format("MMM D h:mma");
        };
    })
    .filter("eventLongDate", function () {
        return function (event) {
            if (!event.DateTime) return null;
            var datetime = moment(event.DateTime);
            return datetime.format("dddd MMM D, YYYY h:mma");
        };
    })
    .filter("eventResults", function () {
        return function (event, viewType) {
            switch (viewType) {
                case 0: // Score
                    if (!event.ScoredPoints && !event.AllowedPoints) return null;
                    return (event.ScoredPoints || "0") + "-" + (event.AllowedPoints || "0");
                case 1: // Tournament
                    if (!event.ScoredPoints && !event.AllowedPoints && !event.TiePoints) return null;
                    return (event.ScoredPoints || "0") + "-" + (event.AllowedPoints || "0") + "-" + (event.TiePoints || "0");
                case 2: // Win, loss
                    break; 
            }
            return null;
        };
    })
    .filter("eventResultsScoreLabel", function () {
        return function (viewType) {
            switch (viewType) {
                case 0: // Score
                    return "Scored";
                case 1: // Tournament
                    return "Wins";
                case 2: // Win, loss
                    break;
            }
            return null;
        };
    })
    .filter("eventResultsAllowedLabel", function () {
        return function (viewType) {
            switch (viewType) {
                case 0: // Score
                    return "Allowed";
                case 1: // Tournament
                    return "Losses";
                case 2: // Win, loss
                    break;
            }
            return null;
        };
    })
    .filter("eventTitle", function () {
        return function (event) {
            
            if (!event.OpponentName) {
                switch (event.Type) {
                    case 1: return "Practice";
                    case 2: return "Meeting";
                    case 3: return "Party";
                    default: return "Untitled";
                }
            }

            var type = "";
            switch (event.Type) {
                case 0: type = "vs. "; break;
                case 1: type = "Practice — "; break;
                case 2: type = "Meeting — "; break;
                case 3: type = "Party — "; break;
            }
            return type + event.OpponentName;
        };
    })
    .filter("eventLocation", function () {
        return function (event, prepend, showArena) {

            if (typeof showArena === "undefined") {
                showArena = true;
            }

            var location = [];
            if (event.Location.Description) {
                location.push(event.Location.Description);
                if (showArena && event.Location.InternalLocation) {
                    location.push(" : " + event.Location.InternalLocation);
                }
            }
            else {
                if (event.Location.Street) {
                    location.push(event.Location.Street);
                }
                if (event.Location.City) {
                    location.push(", " + event.Location.City);
                }
                if (event.Location.Postal) {
                    location.push(", " + event.Location.Postal);
                }
                if (showArena && event.Location.InternalLocation) {
                    location.push(" : " + event.Location.InternalLocation);
                }
            }

            var locationString = location.join('');

            if (!locationString.length) return locationString;
            return (prepend ? prepend + " " : "") + locationString
        };
    })
    .filter("playerName", function () {
        return function (player, showLastName) {

            if (typeof showLastName === "undefined") {
                showLastName = true;
            }

            if (!showLastName) {
                return player.FirstName || "New member";
            }

            if (!player.FirstName && !player.LastName) {
                return "New member";
            }

            var name = [];
            if (player.FirstName) {
                name.push(player.FirstName + " ");
            }
            name.push(player.LastName || "");
            return name.join('').replace(/\s$/, "");
        };
    })
    .filter("playerContact", function () {
        return function (player) {
            var contact = [];
            if (player.Phone) {
                contact.push(player.Phone + " / ");
            }
            contact.push(player.Email || "");
            return contact.join('').replace(/,\s$/, "");
        };
    });