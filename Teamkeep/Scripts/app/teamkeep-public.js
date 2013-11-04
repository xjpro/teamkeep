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
angular.module("teamkeep-public", ["ngRoute", "teamkeep-directives"]);