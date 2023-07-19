using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Drawing;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using WebApplication1.Controllers;
using static WebApplication3.Controllers.SellerController;
using static WebApplication3.Controllers.TestController;
using System.Security.Cryptography;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace WebApplication3
{
    public class DataBase
    { 
       // private string connectString = @"DATA SOURCE=localhost:1521/orcl;TNS_ADMIN=C:\Users\Administrator\Oracle\network\admin;PERSIST SECURITY INFO=True;USER ID=WGY;Password=123456";
        //private string connectString = @"DATA SOURCE=124.70.7.210:1521/orcl;TNS_ADMIN=C:\Users\Administrator\Oracle\network\admin;PERSIST SECURITY INFO=True;USER ID=WGY;Password=123456";
        private string connectString ="User Id=" + "system" + ";Password=" + "wy200286" + ";Data Source=" +"localhost:1521/orcl" + ";";
        OracleConnection con;
        public static DataBase oracleCon;
        public DataBase()
        {
            con = new OracleConnection(connectString);
            con.Open();
            Console.WriteLine("Oracle 连接成功\n");
        }

        public string sqlOperation(string sql, int flag = 0)
        {
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    //Retrieve sample data
                    cmd.CommandText = sql;
                    Console.WriteLine(sql);
                    OracleDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string ans = "";
                        if (flag == 1)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                ans += reader.GetName(i) + "\t";
                            }
                            ans += '\n';
                        }
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            ans += reader[i];
                            if (i != reader.FieldCount - 1)
                                ans += '\t';
                        }
                        //    var ans = reader.GetString(0);
                        reader.Dispose();
                        return ans;

                    }

                    reader.Dispose();
                    return "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }
            }

        }
        public CommodityModel sqlQuerySingleCommodity(string sql) {

            using (OracleCommand cmd = con.CreateCommand())
            {
                var commodityInfo = new CommodityModel();
                try
                {
                    cmd.CommandText = sql;
                    Console.WriteLine(sql);
                    OracleDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    commodityInfo.com_id=reader.GetInt32(0);
                    commodityInfo.com_name=reader.GetString(1);
                    commodityInfo.com_introduction=reader.GetString(2);
                    commodityInfo.com_oriPrice=reader.GetInt32(3);
                    commodityInfo.com_expirationDate=reader.GetDateTime(4).ToString();
                    commodityInfo.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                    commodityInfo.com_left=reader.GetInt32(6);
                    commodityInfo.com_rating = reader.GetDouble(7);
                    commodityInfo.sto_id=reader.GetInt32(8);
                    Console.WriteLine(commodityInfo.ToString());
                    Console.WriteLine("we have find the commodity in func 'sqlQuerySingelCommodity' ");
                    return commodityInfo;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("go wrong in func 'sqlQuerySingleCommodity");
                    Console.WriteLine(ex.Message);
                    return commodityInfo;
                }
            }
        }
        public string sqlQuerySingleCommodityImage(string sql)
        {
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = sql;
                    Console.WriteLine(sql);
                    OracleDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    var fileName=reader.GetString(1);
                    var filePath="D:\\pictures\\"+fileName;
                    Console.WriteLine(filePath);
                    var base64Image = ImageToBase64(filePath);
                    Console.WriteLine("we have find the  image in func 'sqlQuerySingleCommodityImage' ");
                    //记得删，不然
                    return base64Image;
                  
                }
                catch (Exception ex)
                {
                    Console.WriteLine("go wrong in func 'sqlQuerySingleCommodityImage");
                    Console.WriteLine(ex.Message);
                    return "";
                }
            }
        }
        public LoginModel sqlQuerySingelUser(string sql)
        {
            var userInfo=new LoginModel();
            using (OracleCommand cmd = con.CreateCommand())
            {
                //string jsonString = "{\"com_id\":424,\"com_name\":\"name\",\"com_introduction\":\"info\",\"com_left\":12,\"com_oriPrice\":123,\"com_uploadDate\":\"2022-12-04\",\"com_expirationDate\":\"2023-12-04\"}";
                //var json = JsonSerializer.Deserialize<JsonElement>(jsonString);
                try
                {
                    cmd.CommandText = sql;
                    Console.WriteLine(sql);
                    OracleDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    userInfo.username=reader.GetString(0);        
                    Console.WriteLine("in database the user's name:"+ reader.GetString(0) + '\n');
                    userInfo.password=reader.GetString(1);
                    Console.WriteLine("in database the user's password:" + reader.GetString(1) + '\n');
                    userInfo.userType = reader.GetInt32(6);
                    Console.WriteLine("the user exists" + '\n');
                    return userInfo;              
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //userInfo.username = "-1";
                    //userInfo.password = "-1";
                    //在定义处写default赋值，在controller的函数中来判断
                    return userInfo;
                }
            }
        }
        public int sqlInsertSingle(string sql)
        {
            int flag = 1;//返回1表示插入成功，反之表示插入失败；
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = sql;
                    Console.WriteLine("In sqlInsert function gotta excute :" + sql);
                    int rowsUpdated = cmd.ExecuteNonQuery();
                    Console.WriteLine(rowsUpdated + " rows updated");
                    Console.WriteLine("func sqlInsert sucessed!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    flag = -1;
                    Console.WriteLine("func sqlInsert failed!");
                    return flag;
                }
                return flag;
            }
        }
        public int sqlInsertAtomicity(List<string>sql)
        {
            int flag = 1;//返回1表示插入成功，反之表示插入失败；
            OracleTransaction transaction = con.BeginTransaction();
            try
            {
                for(int i = 0; i < sql.Count; i++)
                {
                    var cmd=con.CreateCommand();
                    cmd.CommandText = sql[i];
                    Console.WriteLine("No. "+(i+1)+" sql： "+cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
                Console.WriteLine("we have succesfully executed "+ sql.Count+ " sql sentences");
                return flag;
            }
            catch (Exception ex)
            {
                Console.WriteLine("we failed to execute");
                Console.WriteLine(ex.Message);
                flag = -1;
                transaction.Rollback();
                return flag;
            }

        }
        public string ImageToBase64(string imagePath)
        {
            byte[] image=File.ReadAllBytes(imagePath);
            var str = "data:image/jpeg;base64,"+ Convert.ToBase64String(image);
            return str;
        }
         
        /*
         * 作用：根据传输的com_name 对数据库中的数据进行模糊匹配，传递一个list<nameModel>
         * 输入：com_name
         * 输出：一个list<nameModel>
         */
        public List<nameModel> testGetCommodityByName(string searchName) {
            var list = new List<nameModel>();
            var searchSql = string.Format("SELECT COM_NAME,COM_ID FROM COMMODITY WHERE COM_NAME LIKE '%{0:G}%'", searchName);
            Console.WriteLine("In testGetCommodityByName function going to execute: " + searchSql);
            using(var cmd=con.CreateCommand())
            {
                cmd.CommandText=searchSql;
                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var nameModel = new nameModel();
                    nameModel.com_name = reader.GetString(0);
                    nameModel.com_id = reader.GetDouble(1);
                    list.Add(nameModel);
                }
                reader.Dispose();
            }

            return list;
        }
        
    }
}
