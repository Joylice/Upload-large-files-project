//不支持IE 7 及以下版本
(function ($) {
    $.fn.qfUpload = function (options) {
        // 初始化
        var settings = $.extend({
            size: 2 * 1024 * 1024,

            getTokenUrl:'getToken.php',
            upFileUrl:'upload.php',
            // 文件上传成功执行的方法
            success: function () {

            },
            // 文件上传失败执行的方法
            error: function () {

            },
            // 如果有记录 则存放上传的片数，获取的token 最大片数 没有记录则为NUll
            ready: function (data) {
            }
        }, options);

        var file = $(this).prop('files')[0];
        var filePath = $(this).val();

        var localData = JSON.parse(localStorage.getItem(filePath));
        settings.ready(localData);
        // 没有对上传后修改配置进行检查
        if (localData == null) {
            getTokenAndRun();
        } else {
            run();
        }
        //获取token并运行
        function getTokenAndRun() {
            var fileSlicesSum = Math.ceil((file.size) / settings.size);
            var data = {
                filename: file.name,
                slicessum: fileSlicesSum
            };
            $.get(settings.getTokenUrl, data, function (data) {

                var json_data = {
                    token: data,
                    fileSliceStart: 0,
                    fileSlicesSum: fileSlicesSum
                };
                localStorage.setItem(filePath, JSON.stringify(json_data));
                run();
            });
        }
        function run() {
            var localData = JSON.parse(localStorage.getItem(filePath));
            var fileSlice = getSliceSize(localData.fileSliceStart);
            var fd = new FormData();
            fd.append("token", localData.token);
            fd.append("data", file.slice(fileSlice.start, fileSlice.end));
            fd.append("slice", localData.fileSliceStart);
            $.ajax({
                type: "POST",
                enctype: 'multipart/form-data',
                url: settings.upFileUrl,
                async: true,
                data: fd,
                processData: false,  // tell jQuery not to process the data
                contentType: false,   // tell jQuery not to set contentType
                success: function () {
                    // todo 对服务端返回信息做判断
                    localData.fileSliceStart++;
                    localStorage.setItem(filePath, JSON.stringify(localData));
                    settings.success();
                    //
                    if (fileSlice.end != file.size) {
                        run();
                    } else {
                        // todo check 对上传的文件进行效验
                    }
                }
            });
        }
        function getSliceSize(fileSlice) {
            var ret = {};
            ret.start = fileSlice * settings.size;

            if ((ret.start + settings.size) > file.size) {
                ret.end = file.size;
            } else {
                ret.end = ret.start + settings.size;
            }
            return ret;
        }
        return this;
    };
    $.fn.qfUploadInfo = function()
    {
        var filePath = $(this).val();
        return JSON.parse(localStorage.getItem(filePath));
    }
})(jQuery);