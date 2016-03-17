using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Services.Protocols;
using System.Xml.Linq;

using System.Data.SqlClient;
using System.Data;

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.UI;
using System.IO;
using System.Xml.Serialization;

using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;


namespace iTrip
{
    /// <summary>
    /// SceneryWS 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
     [System.Web.Script.Services.ScriptService]
    public class SceneryWS : System.Web.Services.WebService
    {
        //与SQLServer数据库的连接字符串Data Source=HP-PC\SQLEXPRESS;Initial Catalog=iTip-0414;Integrated Security=True
        private static readonly string sqlConString = @"Data Source=407-33;Initial Catalog=iTip-0414;Integrated Security=True";
       // Data Source=407-33;Initial Catalog=iTip-0414;Integrated Security=True
        //Data Source=HP-PC\SQLEXPRESS;Initial Catalog=iTip-0414;Integrated Security=True
        [Serializable]
        public class HotSpotInfoModel
        {
            public HotSpotInfoModel()
            {
            }
            public int ScenicSpotID { get; set; }//景点ID
            public string ScenicSpotName { get; set; }//景点名称
            public byte[] picture{ get; set; }//景区图片
            public int hotDegree { get; set; }//热度
            public int touristAmount { get; set; }//人流量
        }
        [Serializable]
        public class SpotInfoByIdModel
        {
            public SpotInfoByIdModel()
            {
            }
            public int ScenicSpotID { get; set; }//景点ID
            public string ScenicSpotName { get; set; }//景点名称
            public byte[] picture { get; set; }//景点图片
            public string scenicSpotIntroduce { get; set; }//景点简介
        }

        [XmlInclude(typeof(HotSpotInfoModel))]
        [WebMethod(Description = "Web 服务提供的方法，返回热门景点列表：关于（景点ID、景点名称、景点图片字节流、热度、人流量）的列表")]
        public List<HotSpotInfoModel> getHotSpotList()
        {
            getHotSpotListCode getHSLC = new getHotSpotListCode();
            
            return getHSLC.getHotSpotListDAL();
        }
        [WebMethod(Description = "Web 服务提供的方法，返回给定景点Id的景点信息：景点ID、景点名称、景点图片字节流、景点简介")]
        public string[] getSpotInfoById(int id)
        {
           getSpotInfoByIdCode gsibic=new getSpotInfoByIdCode();
           List<SpotInfoByIdModel> sibim=gsibic.getSpotInfoByIdDAL(id);
           string[] spotInfoString ={"","","",""};
           foreach (SpotInfoByIdModel s in sibim)
           {
               spotInfoString[0] = Convert.ToString(s.ScenicSpotID);
               spotInfoString[1] = s.ScenicSpotName;
               spotInfoString[2] = Convert.ToString(s.picture);
               spotInfoString[3] = s.scenicSpotIntroduce;
           }
           return spotInfoString;

         
        }
        public class getHotSpotListCode
        {
            public getHotSpotListCode()
            {
            }
            public List<HotSpotInfoModel> getHotSpotListDAL()
            {
                DataTable dt = SQLHelper.QueryBySqlText(@"select a.scenicSpotID,a.scenicSpotName,a.picturePath,a.hotDegree, 
                                count(tb_infoStatistics.scenicSpotID)as touristAmount from(
                                select tb_scenicSpot.scenicSpotID, tb_scenicSpot.scenicSpotName,
                                tb_picture.picturePath,tb_scenicSpot.hotDegree
                                from tb_scenicSpot left join tb_picture 
                                on tb_scenicSpot.scenicSpotID=tb_picture.scenicSpotID
                                where tb_scenicSpot.hotDegree>=4  
                                )as a
                                left join tb_infoStatistics on a.scenicSpotID=tb_infoStatistics.scenicSpotID
                                group by a.scenicSpotID,a.scenicSpotName,a.picturePath,a.picturePath,a.hotDegree
                                order by a.scenicSpotID");
                List<HotSpotInfoModel> hsim = new List<HotSpotInfoModel>();
                foreach (DataRow row in dt.Rows)
                {
                    hsim.Add(new HotSpotInfoModel
                    {
                        ScenicSpotID = Convert.ToInt32(row[0]),
                        ScenicSpotName = Convert.ToString(row[1]),
                        picture= GetImage(Convert.ToString(row[2])),
                        hotDegree = Convert.ToInt32(row[3]),
                        touristAmount = Convert.ToInt32(row[4])
                    });
                }
                return hsim;
            }
        }
        public class getSpotInfoByIdCode
        {
            public getSpotInfoByIdCode()
            {}
            public List<SpotInfoByIdModel> getSpotInfoByIdDAL(int id)
            {
                  SqlParameter[] prams = new SqlParameter[]{
                    new SqlParameter("@id",SqlDbType.Int)
                    };
                   prams[0].Value = id;
                DataTable dt = SQLHelper.QueryBySqlText(@"select tb_scenicSpot.scenicSpotID, tb_scenicSpot.scenicSpotName,
                            tb_picture.picturePath,tb_scenicSpot.scenicSpotIntroduce
                            from tb_scenicSpot left join tb_picture 
                            on tb_scenicSpot.scenicSpotID=tb_picture.scenicSpotID
                            where tb_scenicSpot.scenicSpotID=@id
                            order by tb_scenicSpot.scenicSpotID ",prams);
                List<SpotInfoByIdModel> sibim = new List<SpotInfoByIdModel>();
                foreach (DataRow row in dt.Rows)
                {
                    sibim.Add(new SpotInfoByIdModel
                    {
                        ScenicSpotID = Convert.ToInt32(row[0]),
                        ScenicSpotName = Convert.ToString(row[1]),
                        picture =GetImage( Convert.ToString(row[2])),
                        scenicSpotIntroduce = Convert.ToString(row[3])
                    });
                }
                return sibim;
            }
        }

        /// <summary>
        ///  Web 服务提供的方法，返回给定文件的字节数组。
        /// </summary>
        /// <param name="requestFileName"></param>
        /// <returns></returns>
       // [WebMethod(Description = "Web 服务提供的方法，返回给定文件的字节数组")]
        public static byte[] GetImage(string requestFileName)
        {
            ///得到服务器端的一个图片 
            if (requestFileName == null || requestFileName == "")
                return Compress(getBinaryFile("f:\\Picture.JPG"));
            else
                return Compress(getBinaryFile(requestFileName));
        }
        /// <summary>
        /// getBinaryFile：返回所给文件路径的字节数组。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] getBinaryFile(string filename)
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
        public static  byte[] ConvertStreamToByteBuffer(System.IO.Stream theStream)
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
        [WebMethod(Description = "Web 服务提供的方法，返回给定文件类型，目前文件类型限定为jpg")]
        public string GetImageType()
        {
            ///这里只是测试，您可以根据实际的文件类型进行动态输出 
            return "jpg";
        }

        /// <summary>
        /// 压缩指定的字节流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] data)
        {

            //实例化一个内存流对象
            MemoryStream ms = new MemoryStream();
            //实例化一个流对象
            Stream zipStream = null;

            //GZipStream提供用于压缩和解压缩流的方法和属性。

            //CompressionMode.Compress表示压缩
            //ms创建一个压缩流
            zipStream = new GZipStream(ms, CompressionMode.Compress, true);
            //将data中的字节流写入该流中
            zipStream.Write(data, 0, data.Length);

            ///关闭该流
            zipStream.Close();
            //将内存流的位置指向开  始
            ms.Position = 0;
            //声明一个内存流长度的字节数组
            byte[] compressed_data = new byte[ms.Length];
            //从当前流中读取字节流写入compressed_data 中
            ms.Read(compressed_data, 0, int.Parse(ms.Length.ToString()));
            return compressed_data;
        }

        public class SQLHelper
        {
            
            public static SqlConnection connection = new SqlConnection(sqlConString);

            #region   传入参数并且转换为SqlParameter类型
            /// <summary>
            /// 转换参数
            /// </summary>
            /// <param name="ParamName">存储过程名称或命令文本</param>
            /// <param name="DbType">参数类型</param></param>
            /// <param name="Size">参数大小</param>
            /// <param name="Value">参数值</param>
            /// <returns>新的 parameter 对象</returns>
            public SqlParameter MakeInParam(string ParamName, SqlDbType DbType, int Size, object Value)
            {
                return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
            }

            /// <summary>
            /// 初始化参数值
            /// </summary>
            /// <param name="ParamName">存储过程名称或命令文本</param>
            /// <param name="DbType">参数类型</param>
            /// <param name="Size">参数大小</param>
            /// <param name="Direction">参数方向</param>
            /// <param name="Value">参数值</param>
            /// <returns>新的 parameter 对象</returns>
            public SqlParameter MakeParam(string ParamName, SqlDbType DbType, Int32 Size, ParameterDirection Direction, object Value)
            {
                SqlParameter param;

                if (Size > 0)
                    param = new SqlParameter(ParamName, DbType, Size);
                else
                    param = new SqlParameter(ParamName, DbType);

                param.Direction = Direction;
                if (!(Direction == ParameterDirection.Output && Value == null))
                    param.Value = Value;
                return param;
            }
            #endregion

            /// <summary>
            /// 准备SqlCommand对象,该对象默认的执行方式为Sql语句，若要执行存储过程，则在调用该函数后需将
            /// SqlCommand的CommandType改成StoredProcedure
            /// </summary>
            /// <param name="conn">SqlCommand所对应的连接</param>
            /// <param name="cmd">需要准备的SqlCommand对象</param>
            /// <param name="tran">SqlCommand对象所对应的事务</param>
            /// <param name="sqlText">SqlCommand所要执行的Sql语句或存储过程名</param>
            /// <param name="prams">SqlCommand所需要的参数</param>
            private static void PrepareCommand(SqlConnection conn, SqlCommand cmd, SqlTransaction tran, string sqlText, SqlParameter[] prams)
            {
                cmd.Connection = conn;
                if (tran != null)
                {
                    cmd.Transaction = tran;
                }
                cmd.CommandText = sqlText;
                if (prams != null)
                {
                    foreach (SqlParameter p in prams)
                    {
                        if (p != null)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }
                }
            }

            /// <summary>
            /// 通过SQL语句进行查询
            /// </summary>
            /// <param name="sqlText">要执行的查询语句</param>
            /// <param name="prams">该查询语句所需要的参数</param>
            /// <returns>返回查询的数据表</returns>
            public static DataTable QueryBySqlText(string sqlText, SqlParameter[] prams)
            {
                using (SqlConnection conn = new SqlConnection(sqlConString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        PrepareCommand(conn, cmd, null, sqlText, prams);
                        SqlDataReader sdr = null;
                        try
                        {
                            conn.Open();
                            sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        }
                        catch (SqlException e)
                        {
                            throw new Exception(e.Message);
                        }

                        DataTable dt = ConvertSqlDataReaderToDataTable(sdr);
                        return dt;
                    }
                }
            }

            /// <summary>
            /// 通过SQL语句进行查询
            /// </summary>
            /// <param name="sqlText">要执行的查询语句</param>        
            /// <returns>返回查询的数据表</returns>
            public static DataTable QueryBySqlText(string sqlText)
            {
                using (SqlConnection conn = new SqlConnection(sqlConString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        PrepareCommand(conn, cmd, null, sqlText, null);
                        SqlDataReader sdr = null;
                        try
                        {
                            conn.Open();
                            sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        }
                        catch (SqlException e)
                        {
                            throw new Exception(e.Message);
                        }

                        DataTable dt = ConvertSqlDataReaderToDataTable(sdr);
                        return dt;
                    }
                }
            }


            /// <summary>
            /// 通过SQL存储过程进行查询
            /// </summary>
            /// <param name="sqlProc">要执行的存储过程名</param>
            /// <param name="prams">该查询语句所需要的参数</param>
            /// <returns>返回查询的数据集</returns>
            public static DataTable QueryBySqlProc(string sqlProc, SqlParameter[] prams)
            {
                using (SqlConnection conn = new SqlConnection(sqlConString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        PrepareCommand(conn, cmd, null, sqlProc, prams);
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlDataReader sdr = null;
                        try
                        {
                            conn.Open();
                            sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        }
                        catch (SqlException e)
                        {
                            throw new Exception(e.Message);
                        }

                        DataTable dt = ConvertSqlDataReaderToDataTable(sdr);
                        return dt;
                    }
                }
            }

            /// <summary>
            /// 将SqlDataReader转换成DataTable
            /// </summary>
            /// <param name="sdr">要转换的SqlDataReader</param>
            /// <returns>返回转换后的DataTable</returns>
            public static DataTable ConvertSqlDataReaderToDataTable(SqlDataReader sdr)
            {
                if (sdr == null)
                {
                    return null;
                }
                DataTable dt = new DataTable();
                int fieldCount = sdr.FieldCount;
                for (int intCounter = 0; intCounter < fieldCount; ++intCounter)
                {
                    dt.Columns.Add(sdr.GetName(intCounter), sdr.GetFieldType(intCounter));
                }

                object[] objValues = new object[fieldCount];
                dt.BeginLoadData();
                while (sdr.Read())
                {
                    sdr.GetValues(objValues);
                    dt.LoadDataRow(objValues, true);
                }
                sdr.Close();
                dt.EndLoadData();
                return dt;
            }

            /// <summary>
            /// 通过Sql语句执行非查询操作
            /// </summary>
            /// <param name="sqlText">要执行的非查询SQL语句</param>
            /// <param name="prams">参数</param>
            /// <returns>若执行成功，则返回true,否则返回false</returns>
            public static bool ExcuteNonQueryBySqlText(string sqlText, SqlParameter[] prams)
            {
                bool result = false;
                using (SqlConnection conn = new SqlConnection(sqlConString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        PrepareCommand(conn, cmd, null, sqlText, prams);
                        try
                        {
                            conn.Open();
                            result = cmd.ExecuteNonQuery() > 0 ? true : false;
                        }
                        catch (SqlException e)
                        {
                            throw new Exception(e.Message);
                        }
                    }
                }
                return result;
            }


            /// <summary>
            /// 当执行存储过程查询操作需要传回output参数时候用该函数
            /// </summary>
            /// <param name="sqlProc">存储过程名称</param>
            /// <param name="cmd">SqlCommand对象</param>
            /// <param name="prams">存储过程所需要参数</param>
            /// <returns>DataTable</returns>
            public static DataTable ExcuteQueryWithOutputParam(string sqlProc, SqlCommand cmd, SqlParameter[] prams)
            {
                using (SqlConnection conn = new SqlConnection(sqlConString))
                {
                    PrepareCommand(conn, cmd, null, sqlProc, prams);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader sdr = null;
                    try
                    {
                        conn.Open();
                        sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    }
                    catch (SqlException e)
                    {
                        throw new Exception(e.Message);
                    }

                    DataTable dt = ConvertSqlDataReaderToDataTable(sdr);
                    return dt;

                }
            }




            /// <summary>
            /// 通过存储过程执行非查询操作，存储过程如果返回0，则表示存储过程执行成功，否则即失败
            /// </summary>
            /// <param name="sqlProc">存储过程名</param>
            /// <param name="prams">参数</param>
            /// <returns></returns>
            public static bool ExcuteNonQueryBySqlProc(string sqlProc, SqlParameter[] prams)
            {
                bool result = false;
                using (SqlConnection conn = new SqlConnection(sqlConString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        PrepareCommand(conn, cmd, null, sqlProc, prams);
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter p = new SqlParameter("@returnValue", SqlDbType.Int);
                        p.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(p);

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            int returnValue = int.Parse(cmd.Parameters["@returnValue"].Value.ToString());
                            result = returnValue > 0 ? false : true;

                        }
                        catch (SqlException e)
                        {
                            throw new Exception(e.Message);
                        }
                    }
                }
                return result;
            }




            /// <summary>
            /// 通过存储过程执行非查询操作，存储过程如果返回0，则表示存储过程执行成功，否则即失败。当需要
            /// 输出参数的时候调用此函数
            /// </summary>
            /// <param name="sqlProc">存储过程名</param>
            /// <param name="prams">参数</param>
            /// <returns></returns>
            public static bool ExcuteNonQueryBySqlProcWithOutputPram(string sqlProc, SqlCommand cmd, SqlParameter[] prams)
            {
                bool result = false;
                using (SqlConnection conn = new SqlConnection(sqlConString))
                {

                    PrepareCommand(conn, cmd, null, sqlProc, prams);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter p = new SqlParameter("@returnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        int returnValue = int.Parse(cmd.Parameters["@returnValue"].Value.ToString());
                        result = returnValue > 0 ? false : true;

                    }
                    catch (SqlException e)
                    {
                        throw new Exception(e.Message);
                    }

                }
                return result;
            }
        }  
    }
}
