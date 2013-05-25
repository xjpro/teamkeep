angular.module("teamkeep").controller("TutorialController", function ($scope) {
    
    $scope.steps = [
        {
            heading: "Welcome to Teamkeep!",
            content: '<p>Need a tour? This short tutorial will get you started.</p>',
            decline: true,
            next: true,
            target: "#team-banner h1",
            pane: "schedule"
        },
        {
            content: "<p>Click +Game to add an event to your team schedule. You can add the event to an existing season or start a new one.</p>" +
                "<p>Add your first event to continue &mdash;</p>",
            target: "#schedule .table-controls button",
            pane: "schedule",
            side: "topleft",
            nextTrigger: "teamkeep.newevent"
        },
        {
            content: "<p>Excellent! The new event was added to your schedule and the changes were uploaded automatically.</p><p>As you enter in an event's date, location, " +
                "and other information we will automatically save any changes you make.</p>",
            next: true,
            target: "#ui-datepicker-div",
            pane: "schedule"
        },
        {
            heading: "Your team roster",
            content: "<p>The roster section stores information about players on your team.</p>",
            next: true,
            target: "#team-banner h1",
            pane: "roster"
        },
        {
            content: "<p>Click +Player to add a new player to your roster. You can organize players into different groupings such as 'Active' for your main players and 'Subs' for substitues.</p>" +
                "<p>Add your first player to continue &mdash;</p>",
            target: "#roster .table-controls button",
            pane: "roster",
            side: "topleft",
            nextTrigger: "teamkeep.newplayer"
        },
        {
            content: "<p>Nicely done! Just like on the schedule any information you enter here will be automatically saved.</p>",
            next: true,
            target: "#roster table tr:last td:eq(1)",
            pane: "roster",
            side: "topright"
        },
        { 
            content: "<p>The edit button <i class='icon-edit highlight'></i> opens a context menu for a particular player. You can reorganize your roster and remove players entirely in here.</p>",
            next: true,
            target: "#roster table tr:last td:first",
            pane: "roster",
            side: "topright"
        },
        {
            heading: "Availability",
            content: "<p>In the availability section you can mark past and future attendance for your upcoming events.</p>",
            next: true,
            target: "#team-banner h1",
            pane: "availability"
        },
        {
            content: "<p>Click on these boxes to cycle through different attendance options.</p><p>If your event is in the future you can have Teamkeep send your players " +
                "an email requesting their attendance. Their replies will be shown here.</p>",
            next: true,
            target: "#availability tbody tr:eq(1) td.icon:first",
            pane: "availability",
            side: "left"
        },
        {
            heading: "Team messaging",
            content: "<p>Finally, this button here will allow you to send messages to your team. You can send a message to the entire roster or just selected members.</p>" +
                "<p>This can be pretty handy if you're away from your computer and need to email the entire team from your phone.</p>",
            next: true,
            target: "#team-functions .btn:first",
            pane: "schedule",
            side: "left"
        },
        {
            heading: "And that's it!",
            content: "<p>If you need any more help please do " +
                "not hesitate to send us an email at <a href='mailto:info@teamkeep.com'>info@teamkeep.com</a>.</p><p>Thanks for choosing Teamkeep!</p>",
            close: true,
            target: "#team-banner h1",
            pane: "schedule"
        }
    ];

    var stepIndex = -1;
    var currentPane;
    $scope.next = function () {

        if (stepIndex >= 0) {
            $scope.steps[stepIndex].active = false;
        }
        
        var step = $scope.steps[++stepIndex];
        var stepElement = $(".tutorial-step:eq(" + stepIndex + ")");
        
        // Set pane
        if (currentPane != step.pane) {
            var paneLink = $("[href='#" + step.pane + "']");
            if (paneLink.length === 0) {
                return $scope.next();
            }
            paneLink.click();
            currentPane = step.pane;
        }

        step.active = true;

        // Position
        var $target = $(step.target);
        var top = $target.offset().top;
        var left = $target.offset().left;

        if (step.side === "left") {
            top -= stepElement.height() / 2;
            left -= stepElement.width() + 15;
        }
        else if (step.side === "top") {
            //top -= element.height() + $target.height();
        }
        else if (step.side === "topleft") {
            top -= stepElement.height() + $target.height() + 5;
            left -= stepElement.width() - $target.width();
        }
        else if (step.side === "topright") {
            top -= stepElement.height() + $target.height() + 5;
            left += $target.width();
        }
        else {
            console.log(stepElement.height());
            top -= stepElement.height() / 2;
            left += $target.width() + 10;
        }
        step.position = { top: top + "px", left: left + "px" };
        // End position
        
        if (step.nextTrigger) {
            $(document).one(step.nextTrigger, function () {
                $scope.$apply($scope.next);
            });            
        }
    };

    $scope.close = function () {
        $scope.steps[stepIndex].active = false;
    };

    //setTimeout(function () { $scope.$apply($scope.next); }, 800);
});