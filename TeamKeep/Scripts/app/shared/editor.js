angular.module("teamkeep").directive("editor", function () {
    return {
        restrict: "E,A",
        transclude: true,
        replace: true,
        template: "<div class='editor' ng-transclude></div>",
        link: function (scope, element, attrs) {

        }
    };
});