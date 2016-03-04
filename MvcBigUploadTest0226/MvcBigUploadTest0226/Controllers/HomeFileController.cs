using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Web.Common;

namespace MvcBigUploadTest0226.Controllers
{
    public class HomeFileController : Controller
    {
        public string RootFolder
        {
            get
            {
                return @"D:\\upload\\";
            }
        }

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult WebUploader2()
        {
            try
            {
                var isOver = true;
                if (isOver)
                {
                    var result = UploaderHelper.UploadSingleProcess(Request, this.RootFolder);
                    if (result.IsOver)
                    {

                    }
                    return Json(result);
                }
                else
                {
                    var result = new UploaderResult
                    {
                        IsOver = true,
                        Message = "文件已存在于服务器."
                    };
                    return Json(result);
                }
            }
            catch (System.Exception ex)
            {
                var result = new UploaderResult
                {
                    IsOver = true,
                    Message = ex.ToString()
                };
                return Json(result);
            }
        }
        public ActionResult CheckFileIsExists()
        {
            var result = UploaderHelper.ProcessCheck(Request, this.RootFolder);
            return Json(result);
        }
        /// <summary>
        /// 文件块合并
        /// </summary>
        /// <returns></returns>
        public ActionResult MergeFiles()
        {
            try
            {
                UploaderHelper.MergeFiles(Request,this.RootFolder);
                return Json("{hasError:\"false\"}");
            }
            catch (Exception ex)
            {
                return Json("{hasError:\"true\"}");
            }
        }
    }
}
