using MvcBigUploadTest0226.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MvcBigUploadTest0226.Controllers
{
    public class testUploaderController : Controller
    {
        //
        // GET: /testUploader/

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult UploadFile() {
            string token = Request.Form["token"];
            string postedChunk = Request.Form["slice"];
            //判断当前片是否已经上传 
            ChunkInfo chunkInfo= GetGuidChunkCount(token);
            string saveFilePath = "D:\\upload\\Files\\";
            //往文件内追加内容
            byte[] fileByte= System.Text.Encoding.Default.GetBytes(Request.Form["data"]);
            FileStream addFile = new FileStream(saveFilePath + chunkInfo, FileMode.Append, FileAccess.Write);
            BinaryWriter AddWriter = new BinaryWriter(addFile);
            //将上传的分片追加到临时文件末尾
            AddWriter.Write(fileByte);
            //关闭BinaryReader文件阅读器
            AddWriter.Close();
            addFile.Close();
            AddWriter.Dispose();
            addFile.Dispose();
            UpdateChunkInfo(token,Convert.ToInt32(postedChunk));
            return Json("OK");
        }
        /// <summary>
        /// 根据token 获取相关信息
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        ///
        [ChildActionOnly]
        private static ChunkInfo GetGuidChunkCount(string guid)
        {
            try
            {
                var sb = new StringBuilder();
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = "data source=127.0.0.1;initial catalog=CactusDB;password=5471351;persist security info=True;user id=sa;workstation id=127.0.0.1;packet size=4096;Max Pool Size=300;Pooling=true";
                conn.Open();
                SqlCommand com = new SqlCommand();
                com.CommandText = "Select * from chunkinfo where token='" + guid + "'";
                com.Connection = conn;
                ChunkInfo chunkInfo = new ChunkInfo();
                SqlDataReader dr = com.ExecuteReader();
                if (dr.Read())
                {
                    chunkInfo.chunkNum = Convert.ToInt32(dr["chunkNum"]);
                    chunkInfo.fileName = dr["fileName"].ToString();
                }
                conn.Close();
                return chunkInfo;
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        public JsonResult GetToken() {
            string fileName = Request.QueryString["filename"];
            string slicessum = Request.QueryString["slicessum"];
            string MD5Str = fileName + slicessum + DateTime.Now.ToString("yyyyMMddhhmmss");
            string strMD5 = MD5Encrypt(MD5Str);
            RecordChunkMessage(strMD5,fileName,Convert.ToInt32(slicessum));
            return Json(strMD5, JsonRequestBehavior.AllowGet); 
        }
        ///   <summary>
        ///   给一个字符串进行MD5加密
        ///   </summary>
        ///   <param   name="strText">待加密字符串</param>
        ///   <returns>加密后的字符串</returns>
        ///   
        [ChildActionOnly]
        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
            return System.Text.Encoding.Default.GetString(result);
        }
       
        /// <summary>
        /// 记录上传分片的相关信息
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="chunk"></param>
        /// <param name="md5"></param>
        /// <param name="filePath"></param>
        /// 
         [ChildActionOnly]
        public static void RecordChunkMessage(string guid,string fileName, int chunkNum=0,int postedChunk=0)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = "data source=127.0.0.1;initial catalog=CactusDB;password=5471351;persist security info=True;user id=sa;workstation id=127.0.0.1;packet size=4096;Max Pool Size=300;Pooling=true";
            conn.Open();
            SqlCommand com = new SqlCommand();
            com.CommandText = "insert into ChunkInfo values(@guid,@chunkNum,@postedChunk,@fileName)";
            com.Connection = conn;
            com.Parameters.AddRange(new SqlParameter[] { new SqlParameter("@guid", guid), new SqlParameter("@chunk", chunkNum), new SqlParameter("@postedChunk", postedChunk),new SqlParameter("@fileName",fileName)});
            com.ExecuteNonQuery();
            conn.Close();
        }

         [ChildActionOnly]
         public static void UpdateChunkInfo(string guid, int postedChunk)
         {
             SqlConnection conn = new SqlConnection();
             conn.ConnectionString = "data source=127.0.0.1;initial catalog=CactusDB;password=5471351;persist security info=True;user id=sa;workstation id=127.0.0.1;packet size=4096;Max Pool Size=300;Pooling=true";
             conn.Open();
             SqlCommand com = new SqlCommand();
             com.CommandText = "update chunkinfo set postedChunk=@postedChunk where token=@guid";
             com.Connection = conn;
             com.Parameters.AddRange(new SqlParameter[] { new SqlParameter("@guid", guid),  new SqlParameter("@postedChunk", postedChunk) });
             com.ExecuteNonQuery();
             conn.Close();
         }
        
    }
}
