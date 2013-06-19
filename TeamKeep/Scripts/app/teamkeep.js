var teamkeep = angular.module("teamkeep", [])
    .filter("dateTimeAbbrev", function() {
        return function(input) {
            return (input) ? moment(input).format("MMM D, h:mma") : "";
        };
    })
    .filter("fullName", function() {
        return function(player) {
            if (player.FirstName && player.LastName) return player.FirstName + " " + player.LastName;
            return player.FirstName || player.LastName || "[Unnamed member]";
        };
    })
    .directive("completeUpdate", function() {
        return {
            restrict: "A",
            require: "ngModel",
            link: function(scope, element, attrs, ngModelController) {
                if (attrs.type === "radio" || attrs.type === "checkbox") return;

                var updateModel = function() {
                    scope.$apply(function() {
                        ngModelController.$setViewValue(element.val());
                    });
                };

                element.unbind("input keydown change")
                    .bind("blur", updateModel);

                if (element[0].nodeName != "textarea") {
                    element.bind("keydown", function(evt) {
                        if (evt.which === 13) updateModel();
                    });
                }
            }
        };
    })
    .directive("datetimePicker", function() {
        return {
            restrict: 'A',
            link: function(scope, element, attrs) {
                element.datetimepicker({
                    dateFormat: "M d, yy,",
                    timeFormat: "h:mm TT",
                    stepMinute: 5,
                    onClose: function(value) {
                        if (attrs.ngModel) {
                            scope.$apply(function() {
                                eval("scope.$parent." + attrs.ngModel + " = value");
                            });
                        }
                    }
                });
            }
        };
    })
    .directive("whenFocused", function($timeout) {
        return {
            restrict: "A",
            link: function(scope, element, attrs) {
                element.on("focus", "input", function() {
                    scope[attrs.whenFocused] = true;
                });
                element.on("blur", "input", function() {
                    $timeout(function() {
                        if (element.find("input:focus").length === 0) {
                            scope[attrs.whenFocused] = false;
                        }
                    }, 0);
                });
            }
        };
    })
    .directive("stopEvent", function() {
        return {
            restrict: 'A',
            link: function(scope, element, attrs) {
                element.bind(attrs.stopEvent, function(e) {
                    e.stopPropagation();
                });
            }
        };
    })
    .directive("lastTabEvent", function() {
        return {
            restrict: 'A',
            link: function(scope, element, attrs) {
                element.on("keydown", "tr:last td:visible:last input", function(evt) {
                    evt.stopImmediatePropagation();
                    if (evt.keyCode === 9 && !evt.shiftKey) {
                        evt.preventDefault();
                        scope.$apply(function() {
                            eval("scope." + attrs.lastTabEvent);
                        });
                    }
                });
            }
        };
    });