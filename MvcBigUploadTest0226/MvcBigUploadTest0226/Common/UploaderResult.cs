using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Web.Common
{
    /// <summary>
    /// 文件上传消息类
    /// </summary>
    public class UploaderResult
    {
        public UploaderResult()
        {
            this.IsOver = false;
            this.Chunk = 0;
            this.Chunks = 0;
            this.FileExtist = false;
            this.ChunkNum = new List<int>();
        }

        /// <summary>
        /// 文件是否全部上传完成
        /// </summary>
        public bool IsOver { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 如果为分块上传、返回当前的分块索引
        /// </summary>
        public int Chunk { get; set; }

        /// <summary>
        /// 总的分块大小
        /// </summary>
        public int Chunks { get; set; }

        /// <summary>
        /// 文件的MD5码
        /// </summary>
        public string Md5 { get; set; }

        /// <summary>
        /// 上传的文件是否已经存在于服务器
        /// </summary>
        public bool FileExtist { get; set; }

        /// <summary>
        /// 服务器已经存在的区块序号
        /// </summary>
        public List<int> ChunkNum { get; set; }

    }
    public class UploaderHelper
    {
        /// <summary>
        /// 断点续传检测、MD5检测
        /// </summary>
        public static UploaderResult ProcessCheck(HttpRequestBase request, string savepath = null
            , Func<HttpPostedFileBase, string> setfilename = null, Func<string, bool> md5check = null)
        {
            UploaderResult obj = new UploaderResult();
            string tempFilePath = savepath + "temp\\" + request["md5"] + "\\";

            //文件大小
            long size = request.Form["size"] == null ? 0 : Convert.ToInt64(request.Form["size"]);

            //文件分块大小
            long chunksize = request.Form["chunksize"] == null ? 0 : Convert.ToInt64(request.Form["chunksize"]);

            //文件区块总数
            int chunks = chunksize != 0 ? Convert.ToInt32(size / chunksize) : 1;
            int j = 0;
            for (int i = 0; i <= chunks; i++)
            {
                if (File.Exists(tempFilePath + i.ToString()))
                {
                    obj.ChunkNum.Add(i);//服务器已经存在的区块编号
                    j++;
                }
            }
            obj.Message = string.Format("服务器已经存在区块数量{0},总区块数量{1},占比{2}%", j
                , chunks + 1, chunks != 0 && j != 0 ? Convert.ToDouble(Convert.ToDouble(j) / Convert.ToDouble(chunks)) * 100 : 0);
            return obj;
        }
        /// <summary>
        /// 文件上传、保存
        /// </summary>
        /// <param name="request"></param>
        /// <param name="savepath"></param>
        /// <returns></returns>
        public static UploaderResult UploadSingleProcess(HttpRequestBase request, string savepath = null)
        {
            UploaderResult obj = new UploaderResult();
            if (request.Files.Count == 0)
            {
                obj.Message = "请求中不包含文件流信息";
                return obj;
            }
            if (request["size"] == null)
            {
                obj.Message = "文件大小为空";
                return obj;
            }
            string tempFilePath = savepath + "temp\\" + request["md5"] + "\\";
            string saveFilePath = savepath + "files\\";
            if (!Directory.Exists(saveFilePath))
            {
                Directory.CreateDirectory(saveFilePath);
            }
            //判断是否分片，若文件大小不足分片，则直接保存
            if (request.Form.AllKeys.Any(m => m == "chunk"))
            {
                //分片时创建临时文件目录
                if (!Directory.Exists(tempFilePath))
                {
                    Directory.CreateDirectory(tempFilePath);
                }
                //取得chunk和chunks
                int chunk = Convert.ToInt32(request.Form["chunk"]);
                int chunks = Convert.ToInt32(request.Form["chunks"]);
                HttpPostedFileBase file = request.Files[0];
                //根据GUID创建用该GUID命名的临时文件
                file.SaveAs(tempFilePath + chunk.ToString());
                //判断是否所有的分块都已经上传完毕
                string[] fileArr = Directory.GetFiles(tempFilePath);
                if (fileArr.Length == chunks)
                {
                    obj.IsOver = true;
                }
            }
            else
            {
                request.Files[0].SaveAs(saveFilePath + request.Files[0].FileName);
                obj.IsOver = true;
            }
            return obj;
        }

        /// <summary>
        /// 文件合并操作
        /// </summary>
        /// <param name="request"></param>
        /// <param name="savePath"></param>
        public static void MergeFiles(HttpRequestBase request, string savePath)
        {
            string ext = request.Form["fileExt"];
            string fileName = request.Form["fileName"];
            string chunkNum = request.Form["chunkNum"];
            string md5 = request.Form["md5"];
            string tempFilePath = savePath + "\\temp\\" + md5 + "\\";
            string saveFilePath = savePath + "\\files\\" + fileName;
            string[] fileArr = Directory.GetFiles(tempFilePath);
            lock (tempFilePath)
            {
                if (Convert.ToInt32(chunkNum) == fileArr.Length)
                {
                    if (System.IO.File.Exists(saveFilePath))
                    {
                        System.IO.File.Delete(saveFilePath);
                    }
                    FileStream addStream = new FileStream(saveFilePath, FileMode.Append, FileAccess.Write);
                    BinaryWriter AddWriter = new BinaryWriter(addStream);
                    for (int i = 0; i < fileArr.Length; i++)
                    {
                        //以小文件所对应的文件名称和打开模式来初始化FileStream文件流，起读取分割作用
                        FileStream TempStream = new FileStream(tempFilePath + i, FileMode.Open);
                        //用FileStream文件流来初始化BinaryReader文件阅读器，也起读取分割文件作用
                        BinaryReader TempReader = new BinaryReader(TempStream);
                        //读取分割文件中的数据，并生成合并后文件
                        AddWriter.Write(TempReader.ReadBytes((int)TempStream.Length));
                        //关闭BinaryReader文件阅读器
                        TempReader.Close();
                        TempStream.Close();
                    }
                    AddWriter.Close();
                    addStream.Close();
                    AddWriter.Dispose();
                    addStream.Dispose();
                    Directory.Delete(tempFilePath, true);
                }
                //验证文件的MD5，确定文件的完整性 前端文件MD5值与后端合并的文件MD5值不相等
                //if (UploaderHelper.GetMD5HashFromFile(saveFilePath) == md5)
                //{
                //    int i = 1;
                //}
            }
        }
    }
}