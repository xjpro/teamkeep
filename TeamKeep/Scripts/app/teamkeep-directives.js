angular.module("teamkeep-directives", [])
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
            template: "<span class='btn-group' data-toggle='buttons'>" +
                        "<label class='btn btn-default btn-sm' ng-class='{active: model == true}' ng-click='model = true'><input type='radio' />{{trueText}}</label>" +
                        "<label class='btn btn-default btn-sm' ng-class='{active: model == false}' ng-click='model = false'><input type='radio' />{{falseText}}</label>" +
                      "</span>",
            scope: {
                model: "=",
                trueText: '@',
                falseText: '@'
            }
        };
    })
    .directive("spinning", function () {
        return {
            restrict: "A",
            replace: true,
            transclude: true,
            template: "<button ng-transclude><i class='icon-spinner icon-spin' ng-show='spinning'></i> </button>",
            scope: {
                spinning: '='
            }
        };
    })
    .directive("openModalAnd", function() {
        return {
            scope: {
                action: "@openModalAnd"
            },
            link: function (scope, element) {
                element.click(function () {
                    scope.$apply(scope.action);
                    $(element.attr("data-target")).modal("show");
                });
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
                eventsCount: "=eventsCount",
                eventsShown: "=eventsShown"
            },
            controller: function ($scope, $element) {

                function adjust() {
                    var availableSpace = $element.outerWidth();
                    if ($scope.windowWidth < 1000) {
                        availableSpace -= 400;
                        $scope.eventsShown = Math.floor(availableSpace / 25);
                        $element.find(".column-position").hide();

                    } else {
                        availableSpace -= 600;
                        $scope.eventsShown = Math.floor(availableSpace / 25);
                        $element.find(".column-position").show();
                    }
                }

                $rootScope.$watch("windowWidth", function (windowWidth) {
                    adjust();
                });
                $scope.$watch("eventsCount", function (value) {
                    if (!value) {
                        $element.hide();
                    }
                });
            }
        };
    })
    .directive("teamkeepSidebar", function () {
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
    .directive("editDropdown", function () {
        return {
            restrict: "EA",
            link: function (scope, element) {
                var menu = $(element).find(".dropdown-menu");
                menu.click(function (evt) {
                    console.log('c');
                    evt.stopPropagation();
                })
                .keydown(function (evt) {
                    if (evt.which === 13) {
                        $("body").click();
                    }
                });

                $(element).click(function () { // Open even when the box is empty
                    if (!scope.isMobile) {

                        var dropMenu = $(this).find(".dropdown-menu");

                        dropMenu.css({
                            top: "-" + dropMenu.outerHeight() + "px"
                        });

                        if (!dropMenu.is(":visible")) {

                            $(this).find("[data-toggle]").dropdown("toggle");

                        }

                        return false;
                    }
                });
            }
        };
    })
    .directive("recipientsDropdown", function () {
        return {
            restrict: "EA",
            controller: function ($scope, $element) {
                $element.click(function () {

                });
            }
        };
    })
    .directive("createTeamModal", function ($http) {
        return {
            replace: true,
            templateUrl: "/Scripts/app/partials/create-team-modal.html",
            controller: function ($scope, $element, $attrs) {
                $scope.open = $scope.$eval($attrs.createTeamModal);
                $scope.dismissable = $scope.$eval($attrs.dismissable);

                $scope.teamType = "sports";
                $scope.teamPublic = true;
                $scope.teamPopulate = true;
                $scope.createTeam = function () {
                    if (!$scope.teamName || $scope.teamName.length < 3) {
                        $scope.errorMessage = "Please include a team name";
                        return;
                    }
                    $http.post("/teams", {
                        type: $scope.teamType,
                        name: $scope.teamName,
                        makePublic: $scope.teamPublic,
                        prepopulate: $scope.teamPopulate
                    }).success(function (response) {
                        window.location = "/teams/" + response.Id;
                    });
                };

                $scope.$watch("open", function (visible) {

                    var modalKeyboard = $scope.dismissable;
                    var modalBackdrop = $scope.dismissable ? "fade" : "static";

                    $($element).modal({ backdrop: modalBackdrop, keyboard: modalKeyboard, show: visible });
                });

            }
        };
    });