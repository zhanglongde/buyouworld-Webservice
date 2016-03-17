using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

using System.Web.UI;

using System.IO;

/// <summary>
///GetBinaryFile 的摘要说明
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
//若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。 
// [System.Web.Script.Services.ScriptService]
public class GetBinaryFile : System.Web.Services.WebService {

    public GetBinaryFile () {
        //如果使用设计的组件，请取消注释以下行 
        //InitializeComponent(); 
    }

    #region Component Designer generated code 
        //Web 服务设计器所必需的 
        private IContainer components = null; 
        /// <summary> 
        /// 清理所有正在使用的资源。 
        /// </summary> 
        protected override void Dispose( bool disposing ) 
        { 
            if(disposing && components != null) 
            { components.Dispose(); } 
            base.Dispose(disposing); 
        }
    #endregion

    [WebMethod]
    public string HelloWorld() {
        return "Hello World";
    }


        /// <summary>
        ///  Web 服务提供的方法，返回给定文件的字节数组。
        /// </summary>
        /// <param name="requestFileName"></param>
        /// <returns></returns>
        [WebMethod(Description = "Web 服务提供的方法，返回给定文件的字节数组")]
        public byte[] GetImage(string requestFileName)
        {
            ///得到服务器端的一个图片 
            if (requestFileName == null || requestFileName == "")
                return getBinaryFile("f:\\Picture.JPG");
            else 
                return getBinaryFile("f:\\" + requestFileName);
        }
        /// <summary>
        /// getBinaryFile：返回所给文件路径的字节数组。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public byte[] getBinaryFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                { 
                    ///打开现有文件以进行读取。
                    FileStream s = File.OpenRead(filename);
                    return ConvertStreamToByteBuffer(s);
                }
                catch (Exception e)
                { 
                    return new byte[0];
                }
            }
            else 
            { 
                return new byte[0];
            }
        }
       /// <summary>
        /// ConvertStreamToByteBuffer：把给定的文件流转换为二进制字节数组。
       /// </summary>
       /// <param name="theStream"></param>
       /// <returns></returns>
        public byte[] ConvertStreamToByteBuffer(System.IO.Stream theStream)
        {
            int b1;
            System.IO.MemoryStream tempStream = new System.IO.MemoryStream();
            while ((b1 = theStream.ReadByte()) != -1)
            {
                tempStream.WriteByte(((byte)b1));
            }
            return tempStream.ToArray();
        }
        /// <summary>
        /// 返回给定图片文件类型
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Web 服务提供的方法，返回给定文件类型。")]
        public string GetImageType()
        {
            ///这里只是测试，您可以根据实际的文件类型进行动态输出 
            return "jpg";
        }
    
}
