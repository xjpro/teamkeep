/*
 AngularJS v1.2.0-rc.2
 (c) 2010-2012 Google, Inc. http://angularjs.org
 License: MIT
*/
(function (m, g, n) {
    'use strict'; function h(a) { var d = {}; a = a.split(","); var c; for (c = 0; c < a.length; c++) d[a[c]] = !0; return d } function C(a, d) {
        function c(a, b, c, f) { b = g.lowercase(b); if (r[b]) for (; e.last() && s[e.last()];) k("", e.last()); t[b] && e.last() == b && k("", b); (f = u[b] || !!f) || e.push(b); var l = {}; c.replace(D, function (a, b, d, c, e) { l[b] = p(d || c || e || "") }); d.start && d.start(b, l, f) } function k(a, b) {
            var c = 0, k; if (b = g.lowercase(b)) for (c = e.length - 1; 0 <= c && e[c] != b; c--); if (0 <= c) {
                for (k = e.length - 1; k >= c; k--) d.end && d.end(e[k]); e.length =
                c
            }
        } var b, f, e = [], l = a; for (e.last = function () { return e[e.length - 1] }; a;) {
            f = !0; if (e.last() && v[e.last()]) a = a.replace(RegExp("(.*)<\\s*\\/\\s*" + e.last() + "[^>]*>", "i"), function (a, b) { b = b.replace(E, "$1").replace(F, "$1"); d.chars && d.chars(p(b)); return "" }), k("", e.last()); else {
                if (0 === a.indexOf("\x3c!--")) b = a.indexOf("--\x3e"), 0 <= b && (d.comment && d.comment(a.substring(4, b)), a = a.substring(b + 3), f = !1); else if (G.test(a)) { if (b = a.match(w)) a = a.substring(b[0].length), b[0].replace(w, k), f = !1 } else H.test(a) && (b = a.match(x)) &&
                (a = a.substring(b[0].length), b[0].replace(x, c), f = !1); f && (b = a.indexOf("<"), f = 0 > b ? a : a.substring(0, b), a = 0 > b ? "" : a.substring(b), d.chars && d.chars(p(f)))
            } if (a == l) throw I("badparse", a); l = a
        } k()
    } function p(a) { q.innerHTML = a.replace(/</g, "&lt;"); return q.innerText || q.textContent || "" } function y(a) { return a.replace(/&/g, "&amp;").replace(J, function (a) { return "&#" + a.charCodeAt(0) + ";" }).replace(/</g, "&lt;").replace(/>/g, "&gt;") } function z(a) {
        var d = !1, c = g.bind(a, a.push); return {
            start: function (a, b, f) {
                a = g.lowercase(a);
                !d && v[a] && (d = a); d || !0 != A[a] || (c("<"), c(a), g.forEach(b, function (a, b) { var d = g.lowercase(b); !0 != K[d] || !0 === B[d] && !a.match(L) || (c(" "), c(b), c('="'), c(y(a)), c('"')) }), c(f ? "/>" : ">"))
            }, end: function (a) { a = g.lowercase(a); d || !0 != A[a] || (c("</"), c(a), c(">")); a == d && (d = !1) }, chars: function (a) { d || c(y(a)) }
        }
    } var I = g.$$minErr("$sanitize"), x = /^<\s*([\w:-]+)((?:\s+[\w:-]+(?:\s*=\s*(?:(?:"[^"]*")|(?:'[^']*')|[^>\s]+))?)*)\s*(\/?)\s*>/, w = /^<\s*\/\s*([\w:-]+)[^>]*>/, D = /([\w:-]+)(?:\s*=\s*(?:(?:"((?:[^"])*)")|(?:'((?:[^'])*)')|([^>\s]+)))?/g,
    H = /^</, G = /^<\s*\//, E = /\x3c!--(.*?)--\x3e/g, F = /<!\[CDATA\[(.*?)]]\x3e/g, L = /^((ftp|https?):\/\/|mailto:|tel:|#)/i, J = /([^\#-~| |!])/g, u = h("area,br,col,hr,img,wbr"); m = h("colgroup,dd,dt,li,p,tbody,td,tfoot,th,thead,tr"); n = h("rp,rt"); var t = g.extend({}, n, m), r = g.extend({}, m, h("address,article,aside,blockquote,caption,center,del,dir,div,dl,figure,figcaption,footer,h1,h2,h3,h4,h5,h6,header,hgroup,hr,ins,map,menu,nav,ol,pre,script,section,table,ul")), s = g.extend({}, n, h("a,abbr,acronym,b,bdi,bdo,big,br,cite,code,del,dfn,em,font,i,img,ins,kbd,label,map,mark,q,ruby,rp,rt,s,samp,small,span,strike,strong,sub,sup,time,tt,u,var")),
    v = h("script,style"), A = g.extend({}, u, r, s, t), B = h("background,cite,href,longdesc,src,usemap"), K = g.extend({}, B, h("abbr,align,alt,axis,bgcolor,border,cellpadding,cellspacing,class,clear,color,cols,colspan,compact,coords,dir,face,headers,height,hreflang,hspace,ismap,lang,language,nohref,nowrap,rel,rev,rows,rowspan,rules,scope,scrolling,shape,span,start,summary,target,title,type,valign,value,vspace,width")), q = document.createElement("pre"); g.module("ngSanitize", []).value("$sanitize", function (a) {
        var d = [];
        C(a, z(d)); return d.join("")
    }); g.module("ngSanitize").filter("linky", function () { var a = /((ftp|https?):\/\/|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s\.\;\,\(\)\{\}\<\>]/, d = /^mailto:/; return function (c, k) { if (!c) return c; var b, f = c, e = [], l = z(e), h, m, n = {}; g.isDefined(k) && (n.target = k); for (; b = f.match(a) ;) h = b[0], b[2] == b[3] && (h = "mailto:" + h), m = b.index, l.chars(f.substr(0, m)), n.href = h, l.start("a", n), l.chars(b[0].replace(d, "")), l.end("a"), f = f.substring(m + b[0].length); l.chars(f); return e.join("") } })
})(window, window.angular);