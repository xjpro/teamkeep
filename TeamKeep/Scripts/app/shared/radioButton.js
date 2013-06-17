angular.module("teamkeep")
    .directive("radioButton", function () {
        return {
            restrict: 'E,A',
            replace: true,
            template:   "<span class='btn-group' data-toggle='buttons-radio'>" +
                        "<button class='btn btn-small' ng-click='model = !model' ng-class='{active: model}'>{{trueText}}</button>" +
                        "<button class='btn btn-small' ng-click='model = !model' ng-class='{active: !model}'>{{falseText}}</button>" +
                        "</span>",
            scope: {
                model: '=',
                trueText: '@',
                falseText: '@'
            }
        };
    });