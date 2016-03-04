using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcBigUploadTest0226.Common
{
    public class ChunkInfo
    {
        public string token { get; set; }
        public int chunkNum { get; set; }
        public int postedChunk { get; set; }
        public string fileName { get; set; }
    }
}