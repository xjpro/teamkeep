angular.module("teamkeep").service("GroupedTableDecorator", function ($location, Team) {

    this.decorate = function ($scope, options) {
        
        // Required values in options
        // editPageUrl - url to go to when item is clicked in mobile mode
        // collectionName - name of collection, e.g. "Seasons" or "PlayerGroups"
        // collectionNameDisplay - display name of the collection, e.g. "Season" or "Group"
        // itemName - name of items in collection, e.g. "Games" or "Players"
        // itemCollectionId - name of collection id in an item, e.g. "SeasonId" or "GroupId"

        $scope.editItem = function (item) {
            if ($scope.isMobile) {
                $location.path(options.editPageUrl + item.Id);
            }
        };

        $scope.selectedItems = function () {
            return _.filter(_.flatten(Team[options.collectionName], options.itemName), function (item) { return item.Selected; });
        };

        $scope.clearSelectedItems = function () {
            _.each($scope.selectedItems(), function (item) {
                item.Selected = false;
            });
        };

        $scope.addTo = function (collection) {
            if (collection === -1) {
                Team[options.collectionName].addCollection(_.max(Team[options.collectionName], 'Order').Id)
                    .success(function () {
                        //_.defer(function () { $("#schedule tr:last input:first").focus(); });
                        // TODO focus?
                    });
            }
            else if (!collection) {
                Team[options.collectionName].addCollection("Untitled " + options.collectionDisplayName)
                    .success(function () {
                        Team[options.collectionName].addItem(_.last(Team[options.collectionName]).Id)
                            .success(function () {
                                //_.defer(function () { $("#schedule tr:last input:first").focus(); });
                                // TODO focus?
                            });
                    });
            } else {
                Team[options.collectionName].addItem(collection.Id)
                    .success(function () {
                        //_.defer(function () { $("#schedule tbody[season-id='" + season.Id + "'] tr:last input:first").focus(); });
                        // TODO focus?
                    });
            }
        };

        $scope.moveItems = function (collection) {

            if (!collection) {
                Team[options.collectionName].addCollection("Untitled " + options.collectionDisplayName).success(function () {
                    $scope.moveItems(_.last(Team[options.collectionName]));
                });
                return;
            }

            _.each($scope.selectedItems(), function (item) {
                var currentParent = _.find(Team[options.collectionName], function (other) { return other.Id == item[options.itemCollectionId]; });
                currentParent[options.itemName].splice(_.findIndex(currentParent[options.itemName], function (match) { return match.Id == item.Id; }), 1);

                item[options.itemCollectionId] = collection.Id;
                collection[options.itemName].push(item);
            });
            $scope.clearSelectedItems();
        };

        $scope.removeItems = function () {
            _.each($scope.selectedItems(), function (item) {
                Team[options.collectionName].removeItem(item);
            });
            $scope.clearSelectedItems();
        };

        $scope.changeCollectionOrder = function (collection, desiredOrder) {

            if (desiredOrder < 0 || desiredOrder >= Team[options.collectionName].length) return;

            var swapping = _.find(Team[options.collectionName], function (otherCollection) { return otherCollection.Order == desiredOrder; });
            swapping.Order = collection.Order;
            collection.Order = desiredOrder;
        };

        $scope.removeCollection = function (collection) {
            Team[options.collectionName].removeCollection(collection);
        };
    };

});