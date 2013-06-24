var teamkeep = angular.module("teamkeep", [])
    .config(["$routeProvider", function($routeProvider) {
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
    .filter("dateTimeAbbrev", function () {
        return function (input) {
            return (input) ? moment(input).format("MMM D, h:mma") : "";
        };
    })
    .filter("fullName", function () {
        return function (player) {
            if (player.FirstName && player.LastName) return player.FirstName + " " + player.LastName;
            return player.FirstName || player.LastName || "[Unnamed member]";
        };
    })
    .directive("completeUpdate", function () {
        return {
            restrict: "A",
            require: "ngModel",
            link: function (scope, element, attrs, ngModelController) {
                if (attrs.type === "radio" || attrs.type === "checkbox") return;

                var updateModel = function () {
                    scope.$apply(function () {
                        ngModelController.$setViewValue(element.val());
                    });
                };

                element.unbind("input change");

                if (!attrs.readonly) {
                    element.bind("change", updateModel);

                    if (element[0].nodeName != "textarea") {
                        element.bind("keydown", function (evt) {
                            if (evt.which === 13) updateModel();
                            return true;
                        });
                    }
                }
            }
        };
    })
    .directive("datetimePicker", function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.datetimepicker({
                    dateFormat: "M d, yy,",
                    timeFormat: "h:mm TT",
                    stepMinute: 5,
                    onClose: function (value) {
                        if (attrs.ngModel) {
                            scope.$apply(function () {
                                eval("scope.$parent." + attrs.ngModel + " = value");
                            });
                        }
                    }
                });
            }
        };
    })
    .directive("whenFocused", ["$timeout", function ($timeout) {
        return {
            restrict: "A",
            controller: function ($scope, $element, $attrs) {
                $element.on("focus", "input", function () {
                    $scope.$apply(function() {
                        $scope[$attrs.whenFocused] = true;
                    });
                });
                $element.on("blur", "input", function () {
                    $timeout(function () {
                        if ($element.find(":focus").length === 0) {
                            $scope[$attrs.whenFocused] = false;
                        }
                    }, 750);
                });
            }
        };
    }])
    .directive("stopEvent", function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.bind(attrs.stopEvent, function (e) {
                    e.stopPropagation();
                });
            }
        };
    })
    .directive("lastTabEvent", function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.on("keydown", "tr:last td:visible:last input", function (evt) {
                    evt.stopImmediatePropagation();
                    if (evt.keyCode === 9 && !evt.shiftKey) {
                        evt.preventDefault();
                        scope.$apply(function () {
                            eval("scope." + attrs.lastTabEvent);
                        });
                    }
                });
            }
        };
    })
    .directive("autosize", function() {
        return {
            restrict: "A",
            link: function(scope, element) {
                $(element).autosize({ append: "\n" });
            }
        };
    })
    .directive("locationEditor", function() {
        return {
            restrict: "E,A",
            link: function (scope, element) {
                
                $(element).bind("click focus", function (evt) {
                    evt.stopPropagation();

                    if (evt.type === "focusin" && $(evt.target).attr("locid") && $(".editor:visible").length > 0) {
                        $(this).closest("tr").find("td.max input").focus();
                        return;
                    }

                    var clicked = $(this);
                    var editor = clicked.parent().find(".editor");
                    if ($(window).width() > 690) {
                        editor.css({
                            position: "absolute",
                            top: clicked.position().top - editor.outerHeight(),
                            left: clicked.position().left - editor.outerWidth() / 2,
                            marginLeft: "0"
                        });
                    } else {
                        editor.css({
                            position: "fixed",
                            top: "10px",
                            left: "50%",
                            marginLeft: "-" + editor.outerWidth() / 2 + "px"
                        });
                    }

                    editor.show().find("input:first").focus();

                    $(window).bind("mouseup.editor keyup.editor", function (docEvt) {
                        docEvt.stopPropagation();
                        var target = $(docEvt.target);
                        if (target.hasClass("editor") || target.attr("locid") === clicked.attr("locid")) return;
                        if (docEvt.which === 27 || editor.find(":focus").length === 0) {
                            editor.fadeOut("fast");
                            $(window).unbind(".editor");
                        }
                    });
                    $(window).bind("resize.editor", function () {
                        editor.fadeOut("fast");
                        $(window).unbind(".editor");
                    });
                    editor.find("button").one("click.editor", function () { // TODO can this trigger multple?
                        editor.fadeOut("fast");
                        $(window).unbind(".editor");
                    });
                });
            }
        };
    })
    .directive("locationTypeahead", function () {
        return {
            restrict: "E",
            replace: true,
            template: "<input type='text' ng-model='model' complete-update />",
            scope: {
                locations: "=",
                model: "="
            },
            link: function (scope, element) {

                element.typeahead({
                    source: function(query, process) {
                        process(_(scope.locations).pluck("Description").compact().unique().value());
                    },
                    matcher: function(item) {
                        if (item.toLowerCase().indexOf(this.query.trim().toLowerCase()) != -1) {
                            return true;
                        }
                        return false;
                    },
                    updater: function(item) {
                        var selected = scope.locations.find(function(location) { return location.Description == item; });
                        if (selected) {
                            element.val(selected.Description).change();
                            var parent = element.parentsUntil(".editor").parent();
                            parent.find(".js-street").val(selected.Street).change();
                            parent.find(".js-city").val(selected.City).change();
                            parent.find(".js-postal").val(selected.Postal).change();
                            parent.find("button").focus();
                        }
                        return item;
                    },
                    items: 3
                });

            }

            // http://tatiyants.com/how-to-use-json-objects-with-twitter-bootstrap-typeahead/
            /*$("#schedule .location input.js-desc").typeahead({
                source: function (query, process) {
                    var locations = _(ko.toJS(teamViewModel.Locations()));
                    process(locations.pluck("Description").compact().unique().value());
                },
                matcher: function (item) {
                    if (item.toLowerCase().indexOf(this.query.trim().toLowerCase()) != -1) {
                        return true;
                    }
                },
                updater: function (item) {
                    var selected = _.find(ko.toJS(teamViewModel.Locations()), function (location) {
                        return location.Description == item;
                    });
                    if (selected) {
                        var parent = this.$element.parent();
                        parent.find(".js-street").val(selected.Street).change();
                        parent.find(".js-city").val(selected.City).change();
                        parent.find(".js-postal").val(selected.Postal).change();
                        parent.find("button").focus();
                    }
                    return item;
                },
                items: 3
            });*/
        };
    });