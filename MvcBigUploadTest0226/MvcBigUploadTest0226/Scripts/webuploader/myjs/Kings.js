(function ($) {

    if (!$) return;

    var Kings = Kings || {};

    Kings.ObjtoObj = function (newObj, oldObj) {

        if (typeof (oldObj) == 'object') {
            oldObj = $.extend(true, oldObj, newObj);
        } else {
            oldObj = newObj;
        }
        
    };

    Kings.AjaxJson = function (url, data, callback, isAsync) {
        var opts = $.extend({
            type: "POST",
            url: url,
            data: "",
            async: false,
            success: function (result) { }
        }, {
            data: data,
            async: isAsync,
            success: callback
        });
        $.ajax(opts);
    };

    window.Kings = Kings;

})(jQuery);