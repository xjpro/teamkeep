angular.module("teamkeep-public", [])
    .directive("spinner", function () {
        return {
            restrict: 'A',
            replace: true,
            transclude: true,
            template: "<button ng-transclude><i class='icon-spinner icon-spin' ng-show='spinner'></i> </button>",
            scope: {
                spinner: '='
            }
        };
    });