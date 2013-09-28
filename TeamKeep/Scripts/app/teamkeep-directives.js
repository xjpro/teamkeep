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
    .directive("radioButton", function () {
        return {
            restrict: "EA",
            replace: true,
            template:   "<span class='btn-group' data-toggle='buttons-radio'>" +
                        "<button class='btn btn-sm' ng-click='model = !model' ng-class='{active: model}'>{{trueText}}</button>" +
                        "<button class='btn btn-sm' ng-click='model = !model' ng-class='{active: !model}'>{{falseText}}</button>" +
                        "</span>",
            scope: {
                model: '=',
                trueText: '@',
                falseText: '@'
            }
        };
    })
    .directive("availabilityAutosize", function ($rootScope) {
        return {
            restrict: "EA",
            replace: true,
            transclude: true,
            template: "<table ng-transclude></table>",
            scope: {
                eventsShown: "=eventsShown"
            },
            controller: function ($scope, $element) {
                $rootScope.$watch("windowWidth", function (value) {
                    // TODO find values programatically
                    if (value < 450) {
                        if ($scope.eventsShown != 2) {
                            $scope.eventsShown = 2;
                            $("#availability .prev").css({ right: "135px" });
                            $element.find(".position").hide();
                        }
                    }
                    else if (value < 775) {
                        if ($scope.eventsShown != 5) {
                            $scope.eventsShown = 5;
                            $("#availability .prev").css({ right: "255px" });
                            $element.find(".position").hide();
                        }
                    }
                    else if (value < 975) {
                        if ($scope.eventsShown != 7) {
                            $scope.eventsShown = 7;
                            $("#availability .prev").css({ right: "335px" });
                            $element.find(".position").show();
                        }
                    }
                    else if (value < 1175) {
                        if ($scope.eventsShown != 10) {
                            $scope.eventsShown = 10;
                            $("#availability .prev").css({ right: "457px" });
                            $element.find(".position").show();
                        }
                    }
                    else if ($scope.eventsShown != 15) {
                        $scope.eventsShown = 15;
                        $("#availability .prev").css({ right: "655px" });
                        $element.find(".position").show();
                    }
                });
            }
        };
    });