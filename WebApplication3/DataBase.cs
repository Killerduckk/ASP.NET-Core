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
using static WebApplication3.Controllers.SearchController;
using static WebApplication3.Controllers.CommodityController;
using static WebApplication3.Controllers.StoreController;
using System.Security.Cryptography;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using WebApplication3.Controllers;
using Microsoft.VisualBasic;

namespace WebApplication3
{
    public class DataBase
    { 
       // private string connectString = @"DATA SOURCE=localhost:1521/orcl;TNS_ADMIN=C:\Users\Administrator\Oracle\network\admin;PERSIST SECURITY INFO=True;USER ID=WGY;Password=123456";
       // private string connectString = @"DATA SOURCE=124.70.7.210:1521/orcl;TNS_ADMIN=C:\Users\Administrator\Oracle\network\admin;PERSIST SECURITY INFO=True;USER ID=WGY;Password=123456";
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
        public List<CommodityListModel> searchCommodityByName(string searchName,int sortType)
        {       
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<CommodityListModel>();
            var searchSql =
                $"SELECT " +
                //$" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,STO_ID,STO_NAME" +
                $" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY,STORE" +
                $" WHERE COM_NAME LIKE '%{searchName}%' AND COMMODITY.STO_ID=STORE.STO_ID" +
                $" ORDER BY COM_RATING DESC";
            Console.WriteLine("In searchCommodityByName function going to execute: " + searchSql+"\n");
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var searchModel = new CommodityListModel();
                        searchModel.com_id = reader.GetInt32(0);
                        searchModel.com_name = reader.GetString(1);
                        searchModel.com_introduction = reader.GetString(2);
                        searchModel.com_oriPrice = reader.GetDouble(3);
                        searchModel.com_expirationDate = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                        searchModel.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                        searchModel.com_left = reader.GetInt32(6);
                        searchModel.com_rating = reader.GetDouble(7);
                        searchModel.sto_id = reader.GetDouble(8);
                        searchModel.sto_name = reader.GetString(9);
                        using (var cmdFitCategory = con.CreateCommand())
                        {                     
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM COMMODITY_CATEGORIES WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();
                           
                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                              
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }
                        using (var cmdFitFirImage = con.CreateCommand())
                        {
                            var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={searchModel.com_id} ORDER BY COM_ID DESC ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitFirImageSql);
                            cmdFitFirImage.CommandText = fitFirImageSql;
                            OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                            while (readerFitFirImage.Read())
                            {
                                searchModel.com_firstImage = readerFitFirImage.GetString(1);
                                break;
                            }
                           
                            readerFitFirImage.Dispose();
                            Console.WriteLine("finish FitFirImageSql");
                        }
                        using (var cmdFitPrice = con.CreateCommand())
                        {
                            var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={searchModel.com_id} ORDER BY COM_PC_TIME ASC";
                            cmdFitPrice.CommandText = fitPriceSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                            OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();       
                            double subPrice = -1;
                            while (readerFitPrice.Read())
                            {
                               
                                if (readerFitPrice.GetDateTime(1) <= date)
                                {                                   
                                    subPrice = readerFitPrice.GetDouble(2);                                 
                                }
                                else {
                                  
                                    searchModel.com_price = subPrice;
                                    Console.WriteLine($"{searchModel.com_name}找到了现有价格{searchModel.com_price}");
                                  
                                    break;                     
                                }                             
                               
                            }
                            readerFitPrice.Dispose();
                            if (searchModel.com_price == -1)
                                Console.WriteLine($"未找到现有价格\n");
                            readerFitPrice.Dispose();
                            Console.WriteLine("finish fitPriceSql\n");
                        }
                        list.Add(searchModel);
                    }
                    reader.Dispose();
                }
                catch(Exception ex) { 
                    Console.WriteLine("In searchCommodityByName function erorr \n"); 
                    Console.WriteLine(ex.Message); }
            }
            return list;
        }

        public CommodityDetailModel searchCommodityByID(int com_id)
        {
            Console.WriteLine("Get into function searchCommodityByID \n");
            DateTime date = DateTime.Now;          
            var searchSql =
                $"SELECT " +
                $" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY, STORE" +
                $" WHERE COM_ID ={com_id} AND COMMODITY.STO_ID=STORE.STO_ID";          
            Console.WriteLine("In searchCommodityByName function going to execute: " + searchSql + "\n");
            var searchModel = new CommodityDetailModel();
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.Write($"{reader.ToString()}");
                        searchModel.com_id = reader.GetInt32(0);
                        searchModel.com_name = reader.GetString(1);
                        searchModel.com_introduction = reader.GetString(2);
                        searchModel.com_oriPrice = reader.GetDouble(3);
                        searchModel.com_expirationDate = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                        searchModel.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                        searchModel.com_left = reader.GetInt32(6);
                        searchModel.com_rating = reader.GetDouble(7);
                        searchModel.sto_id = reader.GetDouble(8);
                        searchModel.sto_name = reader.GetString(9);
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM COMMODITY_CATEGORIES WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));

                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }
                        using (var cmdFitImage = con.CreateCommand())
                        {
                            var fitImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={searchModel.com_id} ORDER BY COM_ID DESC ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitImageSql);
                            cmdFitImage.CommandText = fitImageSql;
                            OracleDataReader readerFitImage = cmdFitImage.ExecuteReader();
                            while (readerFitImage.Read())
                            {
                                searchModel.com_images.Add(readerFitImage.GetString(1));                              
                            }
                            readerFitImage.Dispose();
                            Console.WriteLine("finish FitImageSql");
                        }
                        using (var cmdFitPrice = con.CreateCommand())
                        {

                            var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={searchModel.com_id} ORDER BY COM_PC_TIME ASC";
                            cmdFitPrice.CommandText = fitPriceSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                            OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                            double subPrice = -1;
                            bool findCurrPrice = false;
                            while (readerFitPrice.Read())
                            {
                                var singlePriceNode = new PriceCurveModel();
                                singlePriceNode.com_pc_price=readerFitPrice.GetDouble(2);
                                singlePriceNode.com_pc_time = readerFitPrice.GetDateTime(1).ToString("yyyy-MM-dd");
                                Console.WriteLine(readerFitPrice.GetDateTime(1).ToString("yyyy-MM-DD"));
                                searchModel.com_prices.Add(singlePriceNode);
                                if (readerFitPrice.GetDateTime(1) <= date &&!findCurrPrice)
                                {
                                    subPrice = readerFitPrice.GetDouble(2);
                                }
                                else if(readerFitPrice.GetDateTime(1) >date && !findCurrPrice)
                                {

                                    searchModel.com_price = subPrice;
                                    Console.WriteLine($"{searchModel.com_name}找到了现有价格{searchModel.com_price}");
                                    findCurrPrice = true;
                                    
                                }

                            }
                            readerFitPrice.Dispose();
                            if (searchModel.com_price == -1)
                                Console.WriteLine($"未找到现有价格\n");
                            readerFitPrice.Dispose();
                            Console.WriteLine("finish fitPriceSql\n");
                        }
                        //break;
             
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchCommodityByName function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            return searchModel;
        }

        public List<StoreListModel> searchStoreByName(string searchName)
        {
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<StoreListModel>();
            var searchSql =
                $"SELECT " +
                //$" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,STO_ID,STO_NAME" +
                $" STO_ID,STO_NAME,STO_INTRODUCTION,USER_ADDRESS" +
                $" FROM USERS,STORE" +
                $" WHERE STO_NAME LIKE '%{searchName}%' AND STORE.STO_ID=USERS.USER_ID";

            Console.WriteLine("In searchStoreByName function going to execute: " + searchSql + "\n");
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var searchModel = new StoreListModel();
                        searchModel.sto_id = reader.GetInt32(0);
                        searchModel.sto_name = reader.GetString(1);
                        searchModel.sto_introduction = reader.GetString(2);
                        searchModel.user_address = reader.GetString(3);
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM STORE_CATEGORIES WHERE STORE_ID = {searchModel.sto_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }
                        using (var cmdFitFirImage = con.CreateCommand())
                        {
                            var fitFirImageSql = $"SELECT * FROM STOREIMAGE WHERE STO_ID={searchModel.sto_id} ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitFirImageSql);
                            cmdFitFirImage.CommandText = fitFirImageSql;
                            OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                            while (readerFitFirImage.Read()) {
                                searchModel.sto_firstImage = readerFitFirImage.GetString(1);
                                break;
                            }
                               
                            readerFitFirImage.Dispose();
                            Console.WriteLine("finish FitFirImageSql");
                        }
                        using (var cmdFitCom = con.CreateCommand())
                        {
                            var fitComSql = $"SELECT COM_ID,COM_NAME,COM_EXPIRATIONDATE FROM COMMODITY WHERE STO_ID ={searchModel.sto_id} ORDER BY COM_RATING";
                            cmdFitCom.CommandText = fitComSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitComSql);
                            OracleDataReader readerFitCom = cmdFitCom.ExecuteReader();
                            int count = 0;
                            while (readerFitCom.Read()&&count<3)
                            {

                                var subCom = new SubCommodityListModel();
                                subCom.com_name = readerFitCom.GetString(1);
                                subCom.com_expirationDate = readerFitCom.GetDateTime(2).ToString("yyyy-MM-dd");
                                int my_com_id = readerFitCom.GetInt32(0); ;
                                using (var cmdFitPrice = con.CreateCommand())
                                {
                                    var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={my_com_id} ORDER BY COM_PC_TIME ASC";
                                    cmdFitPrice.CommandText = fitPriceSql;
                                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                                    OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                                    double subPrice = -1;
                                    while (readerFitPrice.Read())
                                    {
                                        if (readerFitPrice.GetDateTime(1) <= date)
                                        {
                                            subPrice = readerFitPrice.GetDouble(2);
                                        }
                                        else if(readerFitPrice.GetDateTime(1) > date)
                                        {
                                            subCom.com_price = subPrice;
                                            Console.WriteLine($"{subCom.com_name}找到了现有价格{subCom.com_price}");
                                            break;
                                        }

                                    }
                                    readerFitPrice.Dispose();
                                    if (subCom.com_price == -1)
                                        Console.WriteLine($"未找到现有价格\n");
                                    readerFitPrice.Dispose();
                                    Console.WriteLine("finish fitPriceSql\n");
                                }

                                using (var cmdFitFirImage = con.CreateCommand())
                                {
                                    var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={my_com_id} ";
                                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitFirImageSql);
                                    cmdFitFirImage.CommandText = fitFirImageSql;
                                    OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                                    readerFitFirImage.Read();
                                    subCom.com_firstImage = readerFitFirImage.GetString(1);
                                    readerFitFirImage.Dispose();
                                    Console.WriteLine("finish FitFirImageSql");
                                }
                                searchModel.com_list.Add(subCom);
                                count++;
                            }
                        }
                        
                        list.Add(searchModel);
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchStoreByName function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine("finish searchStoreByName function");
            return list;
        }

        public StoreDetailModel searchStoreByID(int sto_id)
        {

            Console.WriteLine("Get tnto searchStoreByID function ");
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<StoreListModel>();
            var searchSql =
                $" SELECT " +
                $" STO_ID,STO_NAME,STO_INTRODUCTION,STO_LICENSEIMG,USER_ADDRESS" +
                $" FROM USERS,STORE" +
                $" WHERE STORE.STO_ID={sto_id} AND STORE.STO_ID=USERS.USER_ID";
            Console.WriteLine("In searchStoreByName function going to execute: " + searchSql + "\n");
            var searchModel = new StoreDetailModel();
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        
                        searchModel.sto_id = reader.GetInt32(0);
                        searchModel.sto_name = reader.GetString(1);
                        searchModel.sto_introduction = reader.GetString(2);
                        searchModel.sto_licenseImg = reader.GetString(3);
                        searchModel.user_address = reader.GetString(4);
                        
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var FitCategorySql = $"SELECT COM_CATEGORY FROM STORE_CATEGORIES WHERE STORE_ID = {searchModel.sto_id}";
                            cmdFitCategory.CommandText = FitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + FitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish FitCategorySql");
                        }
                        using (var cmdFitImage = con.CreateCommand())
                        {
                            var fitImageSql = $"SELECT * FROM STOREIMAGE WHERE STO_ID={searchModel.sto_id} ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitImageSql);
                            cmdFitImage.CommandText = fitImageSql;
                            OracleDataReader readerFitImage = cmdFitImage.ExecuteReader();
                            while (readerFitImage.Read())
                            {
                                searchModel.sto_imageList.Add (readerFitImage.GetString(1));
                                
                            }

                            readerFitImage.Dispose();
                            Console.WriteLine("finish FitFirImageSql");
                        }
                        using (var cmdFitCom = con.CreateCommand())
                        {
                            var fitComSql = $"SELECT COM_ID,COM_NAME,COM_EXPIRATIONDATE FROM COMMODITY WHERE STO_ID ={searchModel.sto_id} ORDER BY COM_RATING";
                            cmdFitCom.CommandText = fitComSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitComSql);
                            OracleDataReader readerFitCom = cmdFitCom.ExecuteReader();
                            while (readerFitCom.Read())
                            {
                                var subCom = new SubCommodityListModel();
                                subCom.com_name = readerFitCom.GetString(1);
                                subCom.com_expirationDate = readerFitCom.GetDateTime(2).ToString("yyyy-MM-dd");
                                int my_com_id = readerFitCom.GetInt32(0); ;
                                //fit
                                using (var cmdFitPrice = con.CreateCommand())
                                {
                                    var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={my_com_id} ORDER BY COM_PC_TIME ASC";
                                    cmdFitPrice.CommandText = fitPriceSql;
                                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                                    OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                                    double subPrice = -1;
                                    while (readerFitPrice.Read())
                                    {
                                        if (readerFitPrice.GetDateTime(1) <= date)
                                        {
                                            subPrice = readerFitPrice.GetDouble(2);
                                        }
                                        else if (readerFitPrice.GetDateTime(1) > date)
                                        {
                                            subCom.com_price = subPrice;
                                            Console.WriteLine($"{subCom.com_name}找到了现有价格{subCom.com_price}");
                                            break;
                                        }

                                    }
                                    readerFitPrice.Dispose();
                                    if (subCom.com_price == -1)
                                        Console.WriteLine($"未找到现有价格\n");
                                    readerFitPrice.Dispose();
                                    Console.WriteLine("finish FitPriceSql\n");
                                }

                                using (var cmdFitFirImage = con.CreateCommand())
                                {
                                    var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={my_com_id} ";
                                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitFirImageSql);
                                    cmdFitFirImage.CommandText = fitFirImageSql;
                                    OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                                    readerFitFirImage.Read();
                                    subCom.com_firstImage = readerFitFirImage.GetString(1);
                                    readerFitFirImage.Dispose();
                                    Console.WriteLine("finish FitFirImageSql");
                                }
                                searchModel.com_list.Add(subCom);
                            }
                        }
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchStoreByID function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine("finish searchStoreByID function");
            return searchModel;
        }
    }
}
