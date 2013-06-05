angular.module("teamkeep").factory("Team", ["$rootScope", "$http", "$timeout", function($rootScope, $http, $timeout) {

    var Team = (window.viewData && window.viewData.Team) ? window.viewData.Team : {
        Id: 0,
        Settings: {}
    };

    Team.uri = "/teams/" + Team.Id + "/" + Team.Name;

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


    // Change tracking

    var timeout = null;
    var changes = {
        clear: function() { this.events = {}; },
        events: {}
    };
    
    var putChanges = function () {
        $http.put(Team.uri, {
            events: _.toArray(changes.events)
        }).success(function () { });
        changes.clear();
    };

    angular.forEach(Team.Seasons, function (season) {
        angular.forEach(season.Games, function (event) {
            $rootScope.$watch(function () { return event; }, function (value, oldValue) {
                if (angular.equals(value, oldValue)) return;
                
                changes.events["event" + event.Id] = event;

                $timeout.cancel(timeout);
                timeout = $timeout(putChanges, 1000);
            }, true);
        });
    });

    

    return Team;
}]);