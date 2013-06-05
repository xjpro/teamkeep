angular.module("teamkeep")
    .directive("radioButton", function () {
        return {
            restrict: 'E,A',
            replace: true,
            template:   "<span class='btn-group' data-toggle='buttons-radio'>" +
                        "<button class='btn btn-small' ng-click='modelTrue()' ng-class='{active: model}'>Public</button>" +
                        "<button class='btn btn-small' ng-click='modelFalse()' ng-class='{active: !model}'>Private</button>" +
                        "</span>",
            scope: {
                model: '='
            },
            link: function(scope) {
                scope.modelTrue = function() {
                    scope.model = true;
                };
                scope.modelFalse = function() {
                    scope.model = false;
                };
            }
        };
    });