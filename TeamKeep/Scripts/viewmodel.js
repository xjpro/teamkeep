
// View model of a Season - a collection of games and events
// grouped together under a common category (year, sport, etc)
var SeasonViewModel = function (data) {
    var me = this;
    ko.mapping.fromJS(data, { "Games": ko.mapping.toViewModel(GameViewModel) }, this);

    $.extend(me, new RowDataModel({
        "PUT": window.viewData.Team.Url + "/seasons/" + me.Id(),
        "DELETE": window.viewData.Team.Url + "/seasons/" + me.Id()
    }));

    this.Selected = function () {
        window.teamScheduleViewModel.SelectedSeason(me);
    };
    this.IncrementOrder = function (incBy) {
        $("#schedule button, #roster i").prop("disabled", true);
        $("#schedule .icon-spin").show();

        var swapping = _.find(window.teamViewModel.Seasons(), function (collection) { return collection.Order() == me.Order() + incBy; });
        if (swapping) {
            swapping.Order(me.Order());
        }
        me.Order(Math.max(0, me.Order() + incBy));
        me.UpdateWithCallback(me, function () {
            window.teamScheduleViewModel.SortCollections();
            $("#schedule button, #schedule i").prop("disabled", false);
            $("#schedule .icon-spin").hide();
        });
    };
};

// View model of a player group - a collection of team members
// grouped together under a common category (active, subs, etc)
var PlayerGroupViewModel = function (data) {
    var me = this;
    ko.mapping.fromJS(data, { "Players": ko.mapping.toViewModel(PlayerViewModel) }, this);

    $.extend(me, new RowDataModel({
        "PUT": window.viewData.Team.Url + "/groups/" + me.Id(),
        "DELETE": window.viewData.Team.Url + "/groups/" + me.Id()
    }));

    this.PlayersWithEmail = ko.computed(function () {
        return _.filter(me.Players(), function (player) { return player.Email() != null && player.Email().length > 0; });
    });
    this.FindPlayer = function (id) {
        return _.find(me.Players(), function(player) { return player.Id() == id; });
    }
    this.Selected = function () {
        window.teamRosterViewModel.SelectedGroup(me);
    };
    this.IncrementOrder = function (incBy) {
        $("#roster button, #roster i").prop("disabled", true);
        $("#roster .icon-spin").show();

        var swapping = _.find(window.teamViewModel.PlayerGroups(), function (collection) { return collection.Order() == me.Order() + incBy; });
        if (swapping) {
            swapping.Order(me.Order());
        }
        me.Order(Math.max(0, me.Order() + incBy));

        me.UpdateWithCallback(me, function () {
            window.teamRosterViewModel.SortCollections();
            $("#roster button, #roster i").prop("disabled", false);
            $("#roster .icon-spin").hide();
        });
    };
};

// View model of a game
// TODO should really be an event
var GameViewModel = function (data) {
    var me = this;
    ko.mapping.fromJS(data, {}, me);

    $.extend(me, new RowDataModel({
        "PUT": "/games/" + me.Id(),
        "DELETE": "/games/" + me.Id()
    }));

    this.DateTimeW3 = ko.computed(function () {
        if (!me.DateTime()) return "";
        var m = moment(me.DateTime());
        return m.format("YYYY-MM-DD") + "T" + m.format("HH:mm:ss");
    });
    this.DateTimeAbbrev = ko.computed(function () {
        if (!me.DateTime()) return "";
        var m = moment(me.DateTime());
        return m.format("MMM D, h:mma");
    });
    this.DateTimeMoment = ko.computed(function() {
        return (me.DateTime()) ? moment(me.DateTime()) : null;
    });
    this.IsPast = ko.computed(function() {
        if (!me.DateTime()) return false;
        return moment(me.DateTime()).isBefore(moment());
    });
    
    this.Changed = function () {
        window.teamScheduleViewModel.Changed();
    };

    this.LocationDisplay = ko.computed(function () {
        if (me.Location != null) {
            if (me.Location.Description()) return me.Location.Description();

            var address = [];
            address.push(me.Location.Street() || "");
            if (me.Location.City()) { address.push(" " + me.Location.City()); };
            if (me.Location.Postal()) { address.push(" " + me.Location.Postal()); };
            return address.join('').trim();
        }
        return "";
    });
    this.LocationEditorEligible = ko.computed(function () {
        var inputCount = 0;
        if (me.Location.Description()) inputCount++;
        if (me.Location.Street()) inputCount++;
        if (me.Location.City()) inputCount++;
        if (me.Location.Postal()) inputCount++;
        return inputCount > 1;
    });
    this.SubLocation = ko.computed(function () {
        return (me.Location == null) ? null : me.Location.InternalLocation();
    });
    this.MoveGame = function (newSeason, element) {

        $(element).parentsUntil("tbody", "tr").fadeOut();

        var oldSeasonId = me.SeasonId();
        me.SeasonId(newSeason.Id());

        $.ajax({
            type: "PUT", url: "/games/" + me.Id(),
            data: ko.mapping.toJSON(me),
            contentType: "application/json",
            success: function () {

                var oldSeason = _.find(window.teamViewModel.Seasons(), function (season) { return season.Id() == oldSeasonId; });
                oldSeason.Games.remove(me);

                _.find(window.teamViewModel.Seasons(), function (season) { return season.Id() == me.SeasonId(); })
                    .Games.push(me);
                window.teamScheduleViewModel.Sort();
            },
            error: function () {
                $(element).parentsUntil("tbody", "tr").show();
            }
        });
    };
    this.MoveGameToNewSeason = function (element) {
        window.teamScheduleViewModel.AddSeason(function () {
            var newSeason = _.last(window.teamViewModel.Seasons());
            me.MoveGame(newSeason, element);
        });
    };
    this.RemoveGame = function () {
        me.Delete();
        var parentSeason = _.find(window.teamViewModel.Seasons(), function (season) { return season.Id() == me.SeasonId(); });
        parentSeason.Games.remove(me);
    };
    
    if (TeamKeep.isMobile) {
        me.DateTime(me.DateTimeW3());
    }
};

// View model of a player
var PlayerViewModel = function (data) {
    var me = this;
    ko.mapping.fromJS(data, {}, this);

    $.extend(me, new RowDataModel({
        "PUT": window.viewData.Team.Url + "/players/" + me.Id(),
        "DELETE": window.viewData.Team.Url + "/players/" + me.Id()
    }));

    this.FullName = ko.computed(function () {
        if (me.FirstName() == null && me.LastName() == null) return "[Unnamed]";
        if (me.FirstName() == null) return me.LastName();
        if (me.LastName() == null) return me.FirstName();
        return me.FirstName() + " " + me.LastName();
    });
    this.FullNameWithPosition = ko.computed(function () {
        return me.FullName() + " [" + me.Position() + "]";
    });
    this.NextAvailability = function(game) {
        var abForEvent = _.find(me.Availability(), function (ab) {
            return ab.EventId() == game.Id();
        });

        var putData;
        var created = false;
        if (abForEvent) {
            if (abForEvent.AdminStatus() + 1 > 3) {
                abForEvent.AdminStatus(0);
            } else {
                abForEvent.AdminStatus(abForEvent.AdminStatus() + 1);
            }

            putData = ko.mapping.toJS(abForEvent);

        } else {
            putData = {
                PlayerId: me.Id(),
                EventId: game.Id(),
                AdminStatus: 1
            };
            created = true;
        }
        
        // Update
        $.ajax({
            type: "PUT",
            url: teamViewModel.Url() + "/players/" + me.Id() + "/availability",
            contentType: "application/json",
            data: JSON.stringify(putData),
            success: function (response) {
                if (created) {
                    me.Availability.push(ko.mapping.fromJS(response));
                }
            }
        });

        return null;
    };
    this.AvailabilityForEvent = function (event) {
        if (!event) return null;
        var availability = _.find(me.Availability(), function (ab) {
            return ab.EventId() == event.Id();
        });
        return availability;
    };
    this.AvailabilityEmailSent = function (game) {
        var availability = me.AvailabilityForEvent(game);
        return (availability && availability.EmailSent() != null) ? moment(parseInt(availability.EmailSent().substr(6))).format("M/D") : null;
    };
    this.AvailabilityIcon = function(game) {
        var availability = me.AvailabilityForEvent(game);

        if (availability) {
            switch (availability.AdminStatus()) {
                case 1: return "icon-thumbs-up";
                case 2: return "icon-ban-circle";
                case 3: return "icon-question-sign";
            }
        }
        return "";
    };
    this.AvailabilityCss = function(game) {
        var availability = me.AvailabilityForEvent(game);
        
        if (availability) {
            switch (availability.RepliedStatus()) {
                case 1: return "going";
                case 2: return "notgoing";
                case 3: return "maybe";
            }
        }
        return "";
    };

    this.Changed = function () {
        window.teamRosterViewModel.Changed();
    };
    this.MovePlayer = function (movingToGroup, element) {

        $(element).parentsUntil("tbody", "tr").fadeOut();

        var oldGroupId = me.GroupId();
        me.GroupId(movingToGroup.Id());

        $.ajax({
            type: "PUT", url: "/teams/" + window.viewData.Team.Id + "/" + window.viewData.Team.Name + "/players/" + me.Id(),
            data: ko.mapping.toJSON(me),
            contentType: "application/json",
            success: function () {
                var oldGroup = _.find(window.teamViewModel.PlayerGroups(), function (group) { return group.Id() == oldGroupId; });
                oldGroup.Players.remove(me);

                _.find(window.teamViewModel.PlayerGroups(), function (group) { return group.Id() == me.GroupId(); })
                    .Players.push(me);
                window.teamRosterViewModel.Sort();
            },
            error: function () {
                $(element).parentsUntil("tbody", "tr").show();
            }
        });
    };
    this.MovePlayerToNewGroup = function (element) {
        window.teamRosterViewModel.AddGroup(function () {
            var newGroup = _.last(window.teamViewModel.PlayerGroups());
            me.MovePlayer(newGroup, element);
        });
    };
    this.RemovePlayer = function () {
        me.Delete();
        var parentGroup = _.find(window.teamViewModel.PlayerGroups(), function (group) { return group.Id() == me.GroupId(); });
        parentGroup.Players.remove(me);
    };
};

var MessageViewModel = function(data) {
    var me = this;
    ko.mapping.fromJS(data, {}, this);

    $.extend(me, new RowDataModel({
        "PUT": window.viewData.Team.Url + "/messages/" + me.Id(),
        "DELETE": window.viewData.Team.Url + "/messages/" + me.Id()
    }));
};

// Master view model, containing attributes of the team (id, name, etc) as well as 
// all its child collections
var TeamViewModel = function(data) {
    ko.mapping.fromJS(data, {
        "Seasons": ko.mapping.toViewModel(SeasonViewModel),
        "PlayerGroups": ko.mapping.toViewModel(PlayerGroupViewModel),
        "Messages": ko.mapping.toViewModel(MessageViewModel)
    }, this);
};