angular.module("teamkeep")
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
                    $scope.$apply(function () {
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
    .directive("autosize", function () {
        return {
            restrict: "A",
            link: function (scope, element) {
                $(element).autosize({ append: "\n" });
            }
        };
    })
    .directive("locationEditor", function () {
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
        // http://tatiyants.com/how-to-use-json-objects-with-twitter-bootstrap-typeahead/
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
                    source: function (query, process) {
                        process(_(scope.locations).pluck("Description").compact().unique().value());
                    },
                    matcher: function (item) {
                        if (item.toLowerCase().indexOf(this.query.trim().toLowerCase()) != -1) {
                            return true;
                        }
                        return false;
                    },
                    updater: function (item) {
                        var selected = scope.locations.find(function (location) { return location.Description == item; });
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
        };
    })
    .directive("availabilityAutosize", ["$timeout", function ($timeout) {
        return {
            restrict: "E,A",
            replace: true,
            transclude: true,
            template: "<table ng-transclude></table>",
            scope: {
                eventsShown: "=eventsShown"
            },
            link: function (scope, element) {

                var resizeTimeout = null;

                $(window).resize(function () {
                    if (resizeTimeout == null) {
                        resizeTimeout = $timeout(function () {
                            var windowWidth = $(window).width();

                            if (windowWidth < 450) {
                                if (scope.eventsShown != 2) {
                                    scope.eventsShown = 2;
                                    $("#availability-prev").css({
                                        right: "135px"
                                    });
                                    element.find(".position").hide();
                                }
                            }
                            else if (windowWidth < 775) {
                                if (scope.eventsShown != 5) {
                                    scope.eventsShown = 5;
                                    $("#availability-prev").css({
                                        right: "255px"
                                    });
                                    element.find(".position").hide();
                                }
                            }
                            else if (windowWidth < 975) {
                                if (scope.eventsShown != 7) {
                                    scope.eventsShown = 7;
                                    $("#availability-prev").css({
                                        right: "335px"
                                    });
                                    element.find(".position").show();
                                }
                            }
                            else if (windowWidth < 1175) {
                                if (scope.eventsShown != 10) {
                                    scope.eventsShown = 10;
                                    $("#availability-prev").css({
                                        right: "457px"
                                    });
                                    element.find(".position").show();
                                }
                            }
                            else if (scope.eventsShown != 15) {
                                scope.eventsShown = 15;
                                $("#availability-prev").css({
                                    right: "655px"
                                });
                                element.find(".position").show();
                            }

                            resizeTimeout = null;
                        }, 100);
                    }
                });
                $(window).trigger("resize");
            }
        };
    }]);