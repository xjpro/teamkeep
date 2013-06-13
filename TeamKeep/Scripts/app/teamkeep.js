var teamkeep = angular.module("teamkeep", [])
    .filter("dateTimeAbbrev", function() {
        return function (input) {
            return (input) ? moment(input).format("MMM D, h:mma") : "";
        };
    })
    .filter("fullName", function() {
        return function (player) {
            if (player.FirstName && player.LastName) return player.FirstName + " " + player.LastName;
            return player.FirstName || player.LastName || "[Unnamed member]";
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
    .directive("stopEvent", function() {
        return {
            restrict: 'A',
            link: function(scope, element, attr) {
                element.bind(attr.stopEvent, function(e) {
                    e.stopPropagation();
                });
            }
        };
    })
    .directive("lastTabEvent", function() {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.on("keydown", "tbody:last > tr:last > td:last input", function (evt) {
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