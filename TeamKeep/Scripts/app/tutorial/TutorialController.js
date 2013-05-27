angular.module("teamkeep").controller("TutorialController", function ($scope, User) {
    
    $scope.steps = [
        {
            heading: "Welcome to Teamkeep!",
            content: '<p>Need a tour? This short tutorial will get you started.</p>',
            decline: true,
            next: true,
            target: "#team-banner h1",
        },
        {
            heading: "Team roster",
            content: "<p>Let's start with the roster section. This is where you'll list the players on your team and their contact information.</p>",
            next: true,
            target: "#team-banner h1",
            pane: "roster"
        },
        {
            content: "<p>Click +Player to add a new player to your roster.</p>" +
                "<p>Players can be organized into different groups. A common approach would be to put your everyday players into an 'Active' group and " +
                "substitutes into a group labeled 'Subs'.</p>" +
                "<p>Add your first player to continue &mdash;</p>",
            target: "#roster .table-controls button",
            glowTarget: "#roster .table-controls button",
            pane: "roster",
            side: "topleft",
            nextTrigger: "teamkeep.newplayer"
        },
        {
            content: "<p>Excellent! A new player was added to your roster and the changes were uploaded automatically.</p>" +
                "<p>As you enter in player names and contact information we will continue to save any changes.</p>" +
                "<p>Fill out as much of your roster as you'd like then click Next to continue the tour &mdash;</p>",
            next: true,
            target: "#team-banner h1",
            pane: "roster"
        },
        {
            content: "<p>The edit button <i class='icon-edit highlight'></i> opens a context menu with additional options for a particular player.</p>",
            next: true,
            target: "#roster tbody tr:eq(1) td:first",
            glowTarget: "#roster tbody i.icon-edit",
            pane: "roster",
            side: "topright"
        },
        {
            heading: "Team schedule",
            content: "<p>Next up is the schedule section. This is where you'll list your team's events.</p>",
            next: true,
            target: "#team-banner h1",
            pane: "schedule"
        },
        {
            content: "<p>Click +Game to add an event to your schedule. " +
                "Events are organized into seasons. You can add the event to an existing season or start a new one.</p>" +
                "<p>Add your first event to continue &mdash;</p>",
            target: "#schedule .table-controls button",
            glowTarget: "#schedule .table-controls button",
            pane: "schedule",
            side: "topleft",
            nextTrigger: "teamkeep.newevent"
        },
        {
            content: "<p>Nicely done! The new event was added to your schedule and the changes were uploaded automatically.</p>" +
                "<p>Fill out as much of your schedule as you'd like then click Next to continue the tour &mdash;</p>",
            next: true,
            target: "#team-functions",
            pane: "schedule",
            side: "left"
        },
        {
            heading: "Availability",
            content: "<p>In the availability section you can mark attendance for events on your schedule. At a glance you can see " +
                "how many players will be attending a particular game, who most often shows up, and know if an important position needs a substitute.</p>",
            next: true,
            target: "#team-banner h1",
            pane: "availability"
        },
        {
            content: "<p>Click the boxes under an event date to cycle through attendance options manually or have Teamkeep send players " +
                "an email requesting their attendance by clicking the blue envelope <i class='icon-envelope highlight'></i> underneath a particular event (for future events only).</p>",
            next: true,
            target: "#availability tbody tr:eq(1) td.icon:first",
            pane: "availability",
            side: "left"
        },
        {
            heading: "Team messaging",
            content: "<p>You can send messages to your team by clicking the 'Message team' button.</p>",
            next: true,
            target: "#team-functions .btn:first",
            glowTarget: "#team-functions .btn:first",
            pane: "schedule",
            side: "left"
        },
        {
            content: "<p>Need to message select members only? Choose who will recieve a message by checking the box next to their name.</p>",
            next: true,
            target: "#compose .player-group:first",
            pane: "compose",
            side: "left"
        },
        {
            heading: "Settings",
            content: "<p>Finally, if you would like to customize your Teamkeep interface, all of the available options are available by clicking the settings button.</p>",
            next: true,
            target: "#team-functions",
            glowTarget: "#team-functions .btn:last",
            pane: "settings",
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
        if (step.pane && currentPane != step.pane) {
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
            left -= stepElement.outerWidth() + 15;
        }
        else if (step.side === "top") {
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
            top -= stepElement.height() / 2;
            left += $target.width() + 10;
        }
        step.position = { top: top + "px", left: left + "px" };
        // End position

        // Highlight a target
        if (step.glowTarget) {

            var i = 0;
            var maxIterations = 3;
            var startGlow = function () {
                $(step.glowTarget).animate({
                    opacity: "0.25"
                }, function () {
                    $(step.glowTarget).animate({
                        opacity: "1"
                    }, function () {
                        if (i++ < maxIterations) {
                            startGlow();
                        }
                    });
                    
                });
            };
            startGlow();
        }
        // End highlight
        
        if (step.nextTrigger) {
            $(document).one(step.nextTrigger, function () {
                $scope.$apply($scope.next);
            });            
        }
    };

    $scope.close = function () {
        $scope.steps[stepIndex].active = false;
        User.Settings.ShowTutorial = false;
        User.saveSettings();
    };

    if (User && User.Settings.ShowTutorial) {
        setTimeout(function () { $scope.$apply($scope.next); }, 800);
    }
});