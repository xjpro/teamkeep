angular.module("teamkeep")
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
    .directive("availabilityAutosize", function ($timeout) {
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
                                    $("#availability .prev").css({
                                        right: "135px"
                                    });
                                    element.find(".position").hide();
                                }
                            }
                            else if (windowWidth < 775) {
                                if (scope.eventsShown != 5) {
                                    scope.eventsShown = 5;
                                    $("#availability .prev").css({
                                        right: "255px"
                                    });
                                    element.find(".position").hide();
                                }
                            }
                            else if (windowWidth < 975) {
                                if (scope.eventsShown != 7) {
                                    scope.eventsShown = 7;
                                    $("#availability .prev").css({
                                        right: "335px"
                                    });
                                    element.find(".position").show();
                                }
                            }
                            else if (windowWidth < 1175) {
                                if (scope.eventsShown != 10) {
                                    scope.eventsShown = 10;
                                    $("#availability .prev").css({
                                        right: "457px"
                                    });
                                    element.find(".position").show();
                                }
                            }
                            else if (scope.eventsShown != 15) {
                                scope.eventsShown = 15;
                                $("#availability .prev").css({
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
    });