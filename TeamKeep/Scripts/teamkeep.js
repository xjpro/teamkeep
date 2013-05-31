﻿
window.TeamKeep = {
    isMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(navigator.userAgent),
    signIn: function(token, redirect) {
        var now = new Date();
        now.setFullYear(now.getFullYear() + 1);
        document.cookie = "teamkeep-token=" + token.AsString + ";path=/;expires=" + now.toGMTString();
        document.location = redirect;
    }
};

$(function () {
    if (window.viewData) {
        if (window.viewData.User) {
            window.userViewModel = ko.mapping.fromJS(window.viewData.User, ko.mapping.toViewModel(UserViewModel));
        }
        if (window.viewData.Team) {
            window.teamViewModel = ko.mapping.fromJS(window.viewData.Team, ko.mapping.toViewModel(TeamViewModel));
        }
        
        $("#alert-modal").fadeAlert();
    }
});

ko.bindingHandlers['editable'] = {
    'update': function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (value && element.hasAttribute("readonly"))
            element.removeAttribute("readonly");
        else if ((!value) && (!element.hasAttribute("readonly")))
            element.setAttribute("readonly", "readonly");
    }
};

// A custom binding to organize sortable/toggleable headers
// Creates <th> column for each of the columns provider in the 'toggleHeader' argument
// with click binding that tell root object to sort on the given sort type
// Note: It is up to the root object to use the given sort type to sort as it sees fit.
ko.bindingHandlers.toggleHeader = {
    update: function(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var columns = allBindingsAccessor().toggleHeader();

        var html = [];
        $.each(columns, function (i, column) {
            html.push("<th class='" + column.CssClass + "' style='display: " + (column.Visible() ? "table-cell" : "none") + "'>");
            html.push("<a sorttype='" + column.SortType + "' title='" + (column.ToolTip || "") + "'>" + column.Name() + "</a>");
            html.push("</th>");
        });
        $(element).empty().append(html.join(''));
        $(element).find("a[sorttype]").click(function () {
            var root = bindingContext.$root;
            if (typeof root.ToggleSort === "function") {
                root.ToggleSort($(this).attr("sorttype"));
            }
        });
    }  
};

var RowDataModel = function (urls) {
    var me = this;

    this.Urls = urls;

    this.ChildAdded = function (domNode) {
        $(domNode.nextSibling).find("input:first").focus();
    };
    this.ChildRemoved = function (domNode) {
        $(domNode).fadeOut("fast");
    };
    this.EnableDateInput = function (koObject, event) {
        var target = $(event.target);
        if (target.attr("readonly")) return;

        var original = target.val();
        target.datetimepicker({
            dateFormat: "M d, yy,",
            timeFormat: "h:mm TT",
            stepMinute: 5,
            onClose: function (value) {
                if (original !== value) {
                    me.Update(koObject, event);
                }
                // destroy datetimepicker?
            }
        }).datetimepicker("show");
    };
    this.Update = function (koObject, event, callback) {
        var model = koObject;
        var input = (event) ? $(event.target) : null;
        if (input) input.fadeTo("medium", 0);

        $.ajax({
            type: "PUT", url: me.Urls["PUT"],
            data: ko.mapping.toJSON(model),
            contentType: "application/json",
            complete: function () {
                if (input) {
                    input.fadeTo("medium", 100);
                }
                if (typeof model.Changed === "function") {
                    model.Changed();
                }
                if (typeof callback === "function") {
                    callback();
                }
            }
        });
    };
    this.UpdateWithCallback = function(model, callback) {
        return me.Update(model, null, callback);
    };
    this.Delete = function () {
        $.ajax({
            type: "DELETE", url: me.Urls["DELETE"], data: {}
        });
    };
};

var SortableCollectionDataModel = function (model, options) {
    
    var defaults = {
        TableElement: null,
        CollectionName: null,
        ItemName: null
    };

    var settings = $.extend({}, defaults, options);

    var updateInterval = null;

    // Columns
    this.ToggleColumnVisible = function (column, event) {
        event.stopPropagation(); // Stop from closing menu
        if (column.Visible() === false) {
            column.Visible(true);
            localStorage["column." + column.SortType + ".visible"] = true;
        } else {
            column.Visible(false);
            localStorage["column." + column.SortType + ".visible"] = false;
        }
    };

    // Sorting
    this.SortCollections = function() {
        this[settings.CollectionName].sort(function(a, b) {
            if (a.Order() > b.Order()) return 1;
            if (a.Order() < b.Order()) return -1;
            return 0;
        });
    };
    this.SortItems = function (items, type, descending) {
        items.sort(function (a, b) {

            var aSortOn = a[type]();
            var bSortOn = b[type]();

            if (aSortOn === null && bSortOn === null) return 0;
            else if (!aSortOn && aSortOn !== 0) return (descending) ? -1 : 1;
            else if (!bSortOn && bSortOn !== 0) return (descending) ? 1 : -1;

            if (typeof aSortOn.toLowerCase === "function") {
                aSortOn = aSortOn.toLowerCase();
            }
            if (typeof bSortOn.toLowerCase === "function") {
                bSortOn = bSortOn.toLowerCase();
            }

            if (aSortOn > bSortOn) return (descending) ? -1 : 1;
            if (aSortOn < bSortOn) return (descending) ? 1 : -1;
            return 0;
        });
    };
    this.SortItemsByDate = function (items, descending) {
        items.sort(function (a, b) {

            var aDateTime = a.DateTime();
            var bDateTime = b.DateTime();

            // Null dates go to top
            if (!aDateTime && !bDateTime) return 0;
            else if (!aDateTime) return (descending) ? 1 : -1;
            else if (!bDateTime) return (descending) ? -1 : 1;

            var aMoment = moment(aDateTime);
            var bMoment = moment(bDateTime);

            if (aMoment.isAfter(bMoment)) return (descending) ? -1 : 1;
            if (aMoment.isBefore(bMoment)) return (descending) ? 1 : -1;
            return 0;
        });
    };
    this.SortBy = function (type, descending) {
        var me = this;
        if (!settings.CollectionName) {
            me.SortItems(this[settings.ItemName], type, descending);
        } else {
            _.each(this[settings.CollectionName](), function (collection) {
                me.SortItems(collection[settings.ItemName], type, descending);
            });
        }
    };
    this.SortByDate = function (descending) {
        var me = this;
        if (!settings.CollectionName) {
            me.SortItemsByDate(this[settings.ItemName], descending);
        }
        else {
            _.each(this[settings.CollectionName](), function (collection) {
                me.SortItemsByDate(collection[settings.ItemName], descending);
            });
        }
    };
    this.Changed = function () {
        // In order to not move the row while the user is interacting with it,
        // we set this interval to continually check for focus on the row before notifying of change
        if (updateInterval == null) {
            updateInterval = setInterval(function () {
                if (settings.TableElement.find(":focus").length === 0) {
                    if (typeof model.Sort === "function") {
                        model.Sort();
                    }
                    clearInterval(updateInterval);
                    updateInterval = null;
                }
            }, 500);
        }
    };

};