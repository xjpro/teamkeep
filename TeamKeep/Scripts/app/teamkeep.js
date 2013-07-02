var teamkeep = angular.module("teamkeep", [])
    .config(["$routeProvider", function ($routeProvider) {
        $routeProvider
            .when("/schedule", {
                templateUrl: "/Scripts/app/partials/schedule.html",
                controller: "ScheduleController"
            })
            .when("/roster", {
                templateUrl: "/Scripts/app/partials/roster.html",
                controller: "RosterController"
            })
            .when("/availability", {
                templateUrl: "/Scripts/app/partials/availability.html",
                controller: "AvailabilityController"
            })
            .when("/availability/:eventId/request", {
                templateUrl: "/Scripts/app/partials/availability-request.html",
                controller: "AvailabilityRequestController"
            })
            .when("/duties", {
                templateUrl: "/Scripts/app/partials/duties.html",
                controller: "DutiesController"
            })
            .when("/messages", {
                templateUrl: "/Scripts/app/partials/messages.html",
                controller: "MessagesController"
            })
            .when("/messages/:messageId", {
                templateUrl: "/Scripts/app/partials/message.html",
                controller: "MessageController"
            })
            .when("/compose", {
                templateUrl: "/Scripts/app/partials/compose.html",
                controller: "ComposeController"
            })
            .when("/settings", {
                templateUrl: "/Scripts/app/partials/settings.html",
                controller: "SettingsController"
            })
            .otherwise({ redirectTo: "/schedule" });
    }])
    .run(["$rootScope", "$location", function ($rootScope, $location) {

        $rootScope.$on("$routeChangeStart", function () {

            if (!viewData.Team.Editable) { // Lock out everything but schedule and roster
                if ($location.path() != "/schedule" || ($location.path() == "/roster" && !viewData.Privacy.Roster)) {
                    $location.path("/schedule");
                }
            }

            $("#team-nav li").removeClass("active");
            $("#team-nav li:has(a[href='#" + $location.path() + "'])").addClass("active");
        });
    }]);