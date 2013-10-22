/* legacy code, todo factor this into the module */
window.Teamkeep = {
    //isMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(navigator.userAgent),
    signIn: function (token, redirect) {
        var now = new Date();
        now.setFullYear(now.getFullYear() + 1);
        document.cookie = "teamkeep-token=" + token.AsString + ";path=/;expires=" + now.toGMTString();
        document.location = redirect;
    }
};

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