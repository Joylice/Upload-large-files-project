/// <reference path="Kings.js" />
(function () {
    Kings.Uploader = function () {

    };
    Kings.Uploader.cloneObject = function (NewObject, OldObject) {
        function clonePrototype() { }
        clonePrototype.prototype = NewObject;
        var obj = new clonePrototype();
        for (var ele in obj) {
            if (typeof (obj[ele]) == "object")
                Kings.ObjtoObj(obj[ele], OldObject[ele]);
            else
                OldObject[ele] = obj[ele];
        }
    }
    Kings.Uploader.prototype.item = {
        disableGlobalDnd: false,         // 是否禁掉整个页面的拖拽功能
        paste: null,                    // 指定监听paste事件的容器,此功能为通过粘贴来添加截屏的图片
        //pick: "picker",                 // 指定选择文件的按钮容器，不指定则不创建按钮。
        pick: {
            id: '#filePicker',
            label: '点击选择文件'
        },
        accept: null,                   // 指定接受哪些类型的文件
        //thumb: {
        //    width: 110,
        //    height: 110,
        //    quality: 70,
        //    allowMagnify: true,
        //    crop: true,
        //    type: 'image/jpeg'
        //},                     // 配置生成缩略图的选项
        compress: {
            width: 1600,
            height: 1600,
            quality: 90,
            allowMagnify: false,
            crop: false,
            preserveHeaders: true,
            noCompressIfLarger: false,
            compressSize: 0
        },                  // 配置压缩的图片的选项
        auto: false,                    // 设置为 true 后，不需要手动调用上传，有文件选择即开始上传
        prepareNextFile: true,          // 是否允许在文件传输时提前把下一个文件准备好
        chunked: true,                  // 是否要分片处理大文件上传
        chunkSize: 1024 * 1024 * 5,     // 如果要分片，分多大一片？ 默认大小为2M
        chunkRetry: 5,                  // 如果某个分片由于网络问题出错，允许自动重传多少次？
        threads: 3,                     // 上传并发数。允许同时最大上传进程数          
        fileVal: "file",                // 设置文件上传域的name
        formData: {},                   // 文件上传请求的参数表，每次发送都会发送此对象中的参数

        //other.Parameter
        startcheckchunk: true,          // 开始上传前检查是否开启
        startcheckserver: null,         // 开始上传前检查服务器地址
        listchunks: {},                 // 列表分片数
        valimd5url: null,               // MD5 校验服务器地址
        log: null                       // 写日志的容器
    };
    Kings.Uploader.prototype.init = function () {
        WebUploader.Uploader.register({
            "before-send": "beforeSend"
        }, {
            beforeSend: function (block) {
                var task = new $.Deferred();
                var listchunk = this.owner.options.listchunks[block.file.name];
                if (listchunk == null && ~listchunk.length)
                {
                    // 分解
                    task.resolve();
                }
                else
                {
                    var hasrect = false;
                    for(var i=0;i<listchunk.length;i++)
                    {
                        if(block.chunk==listchunk[i])
                        {
                            task.reject();  //  拒绝
                            hasrect = true;
                            break;
                        }
                    }
                    if(!hasrect)
                    {
                        task.resolve();
                    }
                }
                return $.when(task);
            }
        });
    };
    Kings.Uploader.prototype.createImage = function (parameter, eventparameter) {
        Kings.Uploader.cloneObject(parameter, this.item);
        this.item.accept = {
            title: 'Images',
            extensions: 'gif,jpg,jpeg,bmp,png',
            mimeTypes: 'image/*'
        };
        this.item.chunked = false;
        this.init();
        var uploader = WebUploader.create(this.item);
        Kings.Uploader.bind(eventparameter, uploader,this);
        return uploader;
    };
    Kings.Uploader.prototype.createFile = function (parameter, eventparameter) {
        Kings.Uploader.cloneObject(parameter, this.item);
        this.item.accept = {
            title: 'Files',
            mimeTypes: '*'
        };
        this.init();
        var uploader = WebUploader.create(this.item);
        Kings.Uploader.bind(eventparameter, uploader,this);
        return uploader;
    };
    Kings.Uploader.bind = function (eventparameter, uploader,kingsuploader) {
        if (kingsuploader.item.startcheckchunk) {
            uploader.on("uploadStart", function (file) {

                var formData = {
                    size: file.size,
                    startcheckchunk: true,
                    loaded: file.loaded,
                    filename: file.name,
                    id: file.id,
                    chunkNum:file.chunkNum,
                    ext: file.ext,
                    lastModifiedDat: file.lastModifiedDat,
                    chunksize: kingsuploader.item.chunkSize,
                    md5: uploader.options.formData.md5
                };
                Kings.AjaxJson(kingsuploader.item.startcheckserver || kingsuploader.item.server, formData,
                    function (result) {
                        uploader.options.listchunks[file.name] = result.ChunkNum;
                        if (Object.prototype.toString.call(uploader.options.log).slice(8, -1) == "Function") {
                            uploader.options.log(result);
                        }else console.log(result.Message);
                    }, false);//ajax注意使用同步
            });
        };
        for (var ev in eventparameter) {
            uploader.on(ev, eventparameter[ev]);
        }
    }
})();

