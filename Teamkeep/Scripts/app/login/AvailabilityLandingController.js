angular.module("teamkeep-public").controller("AvailabilityLandingController", ["$scope", "$http", function ($scope, $http) {

    var abData = window.viewData.AvailabilityRequest.Data;

    $scope.availabilities = [
        { value: 1, text: "I'll be there!" },
        { value: 2, text: "Can't make it" },
        { value: 3, text: "Maybe" }
    ];

    $scope.selectAvailability = function (selectedAvailability) {

        _.each($scope.availabilities, function(ab) { ab.selected = false; });
        selectedAvailability.selected = true;

        $http.put("/rsvp", {
            token: abData.Token,
            repliedStatus: selectedAvailability.value
        });
    };

    if (abData.RepliedStatus) {
        _.find($scope.availabilities, function(ab) { return ab.value == abData.RepliedStatus; }).selected = true;
    }

    var reply = /reply=(\d{1})/.exec(location.search);
    if (reply) {
        reply = reply[1];
        var initAvailability = _.find($scope.availabilities, function (ab) { return ab.value == reply; });
        $scope.selectAvailability(initAvailability);
    }

}]);