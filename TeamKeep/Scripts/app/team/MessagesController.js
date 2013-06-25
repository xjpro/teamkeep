angular.module("teamkeep").controller("MessagesController", ["$scope", "Team", function ($scope, Team) {

    $scope.messages = Team.Messages;
    $scope.predicate = function(item) {
        return new Date(item.DateTime);
    };
    $scope.timePast = function(message) {
        return moment(message.Date).fromNow();
    };
    $scope.abbreviatedContent = function(message) {
        var length = message.Content.length;
        var abbrev = message.Content.substr(0, 100).replace(/<[^\/]+\/>|<[^>]+>/gi, "");
        return (length > 100) ? abbrev + ". . ." : abbrev;
    };

}]);