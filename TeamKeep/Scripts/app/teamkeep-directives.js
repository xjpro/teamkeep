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
            restrict: "A",
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
    .directive("availabilityAutosize", function ($rootScope, $timeout) {
        return {
            restrict: "A",
            replace: true,
            transclude: true,
            template: "<table ng-transclude></table>",
            scope: {
                eventsCount: "=eventsCount",
                eventsShown: "=eventsShown"
            },
            link: function (scope, element) {

                function adjust() {

                    var availableSpace = element.outerWidth();

                    if ($rootScope.windowWidth < 350) {
                        scope.eventsShown = 2;
                        element.find(".column-position").hide();
                    }
                    else if ($rootScope.windowWidth < 1000) {
                        availableSpace -= 290;
                        scope.eventsShown = Math.floor(availableSpace / 25);
                        element.find(".column-position").hide();

                    } else {
                        availableSpace -= 550;
                        scope.eventsShown = Math.floor(availableSpace / 25);
                        element.find(".column-position").show();
                    }
                }

                $rootScope.$watch("windowWidth", function (windowWidth) {
                    adjust();
                });
                scope.$watch("eventsCount", function (value) {
                    if (!value) {
                        element.hide();
                    }
                });

                $timeout(function () {
                    adjust(); // Initial trigger after everything is drawn
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
            restrict: "A",
            link: function (scope, element) {
                var menu = $(element).find(".dropdown-menu");
                menu.click(function (evt) {
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
    })
    .directive("bannerModal", function ($http, Team) {
        return {
            replace: true,
            templateUrl: "/Scripts/app/partials/banner-modal.html",
            controller: function ($scope, $element, $attrs) {

                $scope.editBanner = function () {

                    if ($("#banner-file").val().length === 0) {
                        $scope.errorMessage = "Please select a valid image file";
                        return;
                    }

                    $scope.errorMessage = "";
                    $scope.bannerUploading = true;

                    var file = document.getElementById("banner-file").files[0];

                    // http://holyhoehle.wordpress.com/2010/09/19/uploading-files-using-ajax/
                    // TODO use $http instead
                    $.ajax({
                        type: "PUT", url: Team.uri + "/banner",
                        processData: false,
                        data: file,
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("Cache-Control", "no-cache");
                            xhr.setRequestHeader("X-File-Name", file.name);
                            xhr.setRequestHeader("Content-Type", "multipart/form-data");
                        },
                        success: function () {
                            window.location.reload();
                        },
                        error: function (response) {
                            $scope.$apply(function () {
                                $scope.errorMessage = $scope.$eval(response.responseText);
                                $scope.bannerUploading = false;
                            });
                        }
                    });
                };
            }
        };
    });