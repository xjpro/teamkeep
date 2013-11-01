angular.module("teamkeep", ["ngRoute", "ngSanitize", "teamkeep-directives", "ui.bootstrap", "ui.clockpicker"])
    .config(function ($routeProvider) {
        $routeProvider
            .when("/schedule", {
                templateUrl: "/Scripts/app/partials/schedule.html",
                controller: "ScheduleController"
            })
            .when("/schedule/events/:eventId", {
                templateUrl: "/Scripts/app/partials/schedule-event.html",
                controller: "ScheduleEventController"
            })
            .when("/roster", {
                templateUrl: "/Scripts/app/partials/roster.html",
                controller: "RosterController"
            })
            .when("/roster/members/:memberId", {
                templateUrl: "/Scripts/app/partials/roster-member.html",
                controller: "RosterMemberController"
            })
            .when("/availability", {
                templateUrl: "/Scripts/app/partials/availability.html",
                controller: "AvailabilityController"
            })
            .when("/messages", {
                templateUrl: "/Scripts/app/partials/messages.html",
                controller: "MessagesController"
            })
            .when("/compose", {
                templateUrl: "/Scripts/app/partials/messages-compose.html",
                controller: "MessagesComposeController"
            })
            .when("/settings", {
                templateUrl: "/Scripts/app/partials/settings.html",
                controller: "SettingsController"
            })
            .when("/user", {
                templateUrl: "/Scripts/app/partials/user-settings.html",
                controller: "UserSettingsController"
            })
            .otherwise({ redirectTo: "/schedule" });
    })
    .run(function ($rootScope, $window, $location, Team) {

        //$rootScope.publicView = User.Id == 0 || User
        $rootScope.windowWidth = $window.outerWidth;
        $rootScope.isMobile = $rootScope.windowWidth < 767;
        angular.element($window).bind("resize", function () {
            $rootScope.windowWidth = $window.outerWidth;
            $rootScope.isMobile = $rootScope.windowWidth < 767;
            $rootScope.$apply("windowWidth");
        });

        $rootScope.toggleSidebar = function () {
            $rootScope.$broadcast("$toggleSidebar");
        };

        $rootScope.$on("$routeChangeSuccess", function (ngEvent, next) {
            if (!next || !next.$$route) return;
            var matches = /\/([^\/]*)/.exec(next.$$route.originalPath);
            var firstPath = matches[1];

            if (!Team.Editable) {
                if (firstPath == "schedule" || (firstPath == "roster" && Team.Privacy.Roster)) {
                    return;
                }
                $location.path("/schedule");
            }
        });
    });