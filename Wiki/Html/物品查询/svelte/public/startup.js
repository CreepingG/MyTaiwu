/* eslint-disable no-empty-function */
/* eslint-disable no-multi-assign */
/* eslint-disable no-undef */
window.mwPerformance = (window.performance && performance.mark) ? performance : {
    mark: function () {}
};
window.mwNow = (function () {
    let perf = window.performance,
        navStart = perf && perf.timing && perf.timing.navigationStart;
    return navStart && typeof perf.now === 'function' ? function () {
        return navStart + perf.now();
    } : function () {
        return Date.now();
    };
})();
window.isCompatible = function (str) {
    let ua = str || navigator.userAgent;
    return !!((function () {
        return !this && !!Function.prototype.bind && !!window.JSON;
    })() && 'querySelector' in document && 'localStorage' in window && 'addEventListener' in window && !(ua.match(/webOS\/1\.[0-4]|SymbianOS|Series60|NetFront|Opera Mini|S40OviBrowser|MeeGo|Android.+Glass|^Mozilla\/5\.0 .+ Gecko\/$|googleweblight/) || ua.match(/PlayStation/i)));
};
(function () {
    let NORLQ, script;
    if (!isCompatible()) {
        document.documentElement.className = document.documentElement.className.replace(/(^|\s)client-js(\s|$)/, '$1client-nojs$2');
        NORLQ = window.NORLQ || [];
        while (NORLQ.length) {
            NORLQ.shift()();
        }
        window.NORLQ = {
            push: function (
                fn) {
                fn();
            }
        };
        window.RLQ = {
            push: function () {}
        };
        return;
    }

    function startUp() {
        mw.config = new mw.Map(true);
        mw.loader.addSource({
            "local": "https://cdn.huijiwiki.com/taiwu/load.php"
        });
    }
    window.mediaWikiLoadStart = mwNow();
    mwPerformance.mark('mwLoadStart');
    script = document.createElement('script');
    script.src = "https://cdn.huijiwiki.com/taiwu/load.php?debug=false&lang=zh-cn&modules=jquery%2Cmediawiki&only=scripts&skin=bootstrapmediawiki&version=04n4x5u";
    script.onload = script.onreadystatechange = function () {
        if (!script.readyState || /loaded|complete/.test(script.readyState)) {
            script.onload = script.onreadystatechange = null;
            script = null;
            startUp();
        }
    };
    document.getElementsByTagName('head')[0].appendChild(script);
})();