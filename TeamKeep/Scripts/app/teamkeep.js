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
    .run(function ($rootScope, $window) {
        
        $rootScope.windowWidth = $window.outerWidth;
        $rootScope.isMobile = $rootScope.windowWidth < 767;
        angular.element($window).bind("resize", function() {
            $rootScope.windowWidth = $window.outerWidth;
            $rootScope.isMobile = $rootScope.windowWidth < 767;
            $rootScope.$apply("windowWidth");
        });

        $rootScope.toggleSidebar = function() {
            $rootScope.$broadcast("$toggleSidebar");
        };
    })
    .directive("teamkeepSidebar", function() {
        return function (scope, element, attrs) {

            scope.$watch(attrs.teamkeepSidebar, function (value) {
                if (value) {
                    $(element).show();
                    $("body").addClass("pushed");
                } else {
                    $(element).hide();
                    $("body").removeClass("pushed");
                }
            });

        };
    })
    .directive("editDropdown", function() {
        return {
            restrict: "E,A",
            link: function(scope, element) {
                var menu = $(element).find(".dropdown-menu");
                menu.click(function(evt) {
                    evt.stopPropagation();
                })
                .keydown(function(evt) {
                    if (evt.which === 13) {
                        $("body").click();
                    }
                });

                $(element).click(function () { // Open even when the box is empty
                    if (!scope.isMobile) {

                        $(element).find(".dropdown-menu").css({
                            top: "-" + menu.outerHeight() + "px"
                        });

                        $(this).find("[data-toggle]").dropdown("toggle");

                        return false;
                    }
                });
            }
        };
    })
    .directive("navDropdown", function () {
        return {
            restrict: "E,A",
            link: function (scope, element) {
                var menu = $(element).find(".dropdown-menu");
                menu.css({
                    left: "-" + menu.width() + "px"
                });
            }
        };
    });